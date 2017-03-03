using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {
    public class Grid : MonoBehaviour {

        [SerializeField]
        int m_width = 0, m_height = 0;

        [SerializeField]
        Tile m_tilePrefab;
        List<Tile> m_grid;

        bool m_hasBeenInitialized = false;
        Dictionary<Tile, TileStateEnum> m_accessibleTiles = new Dictionary<Tile, TileStateEnum>();

        void Awake() {
            //      InitializeGrid();
        }

        // Use this for initialization
        void Start() {
            InitializeGrid();
        }

        public void InitializeGrid() {
            if (m_hasBeenInitialized) return;

            float x_offset = (m_width / 2f) - .5f;
            float y_offset = (m_height / 2f) - .5f;

            m_grid = new List<Tile>();
            for (int i = 0; i < m_height; i++) { // row
                for (int j = 0; j < m_width; j++) { // col

                    Tile go = GameObject.Instantiate<Tile>(m_tilePrefab);

                    go.xPos = j;

                    go.yPos = i;

                    go.gameObject.name = "Tile_" + j + "_" + i;
                    go.SetParentGrid(this);

                    Transform tTransform = go.transform;

                    tTransform.SetParent(transform);
                    tTransform.localPosition = Vector3.zero;
                    tTransform.Translate(j - x_offset, i - y_offset, 0);

                    m_grid.Add(go);
                }
            }

            m_hasBeenInitialized = true;
        }

        public Tile GetTileAt(int row, int col) {
            if (row < 0 || col < 0 || row >= m_width || col >= m_height) {
                return null;
            }
            //return m_grid[(row * m_width) + col];
            return m_grid[(col * m_height) + row];
        }

        private void FillTileAdjacency(int TileX, int TileY, int MovesLeft, int attackRange, bool initCall = false) {
            Tile tileAt = GetTileAt(TileX, TileY);
            if (tileAt == null) return;
            if (initCall) {
                //Debug.Log(string.Format("FillTileAdjacency: ({0}, {1})", TileX, TileY));
                m_accessibleTiles.Add(tileAt, TileStateEnum.CanNotAccess);
            }

            int dm = 1;
            int da = 0;

            if (MovesLeft == 0) {
                if (attackRange > 0) {
                    dm = 0;
                    da = attackRange;
                }
                else {
                    // add if empty
                    if (!m_accessibleTiles.ContainsKey(tileAt)) {
                        if (tileAt.GetPlaceable() == null) {
                            m_accessibleTiles.Add(tileAt, TileStateEnum.CanAttack);
                        }
                        else {
                            // Possibly need to change this to can attack if there is a pathable location to attack this object.
                            m_accessibleTiles.Add(tileAt, TileStateEnum.CanNotAccess);
                        }
                    }
                    return;
                }
            }
            if (!initCall && (MovesLeft > 0 || attackRange > 0)) {

                TileStateEnum stateToSet = (tileAt.GetPlaceable() == null) ? TileStateEnum.CanMove : TileStateEnum.CanNotAccess;

                if (!m_accessibleTiles.ContainsKey(tileAt)) {
                    m_accessibleTiles.Add(tileAt, stateToSet);
                }
                else if (m_accessibleTiles[tileAt] == TileStateEnum.CanAttack) { // || m_accessibleTiles[tileAt] == TileStateEnum.CanNotAccess) {
                    m_accessibleTiles[tileAt] = stateToSet;
                }
            }

            if (initCall || m_accessibleTiles.ContainsKey(tileAt) && m_accessibleTiles[tileAt] == TileStateEnum.CanMove) {
                // LEFT
                FillTileAdjacency(TileX - dm - da, TileY, MovesLeft - dm, attackRange - da);

                // UP
                FillTileAdjacency(TileX, TileY + dm + da, MovesLeft - dm, attackRange - da);

                // RIGHT
                FillTileAdjacency(TileX + dm + da, TileY, MovesLeft - dm, attackRange - da);

                // BOT
                FillTileAdjacency(TileX, TileY - dm - da, MovesLeft - dm, attackRange - da);
            }

            return;
        }

        // (-1, 0), (0, 1), (0, -1), (1,0)
        private List<Tile> GetCircumference(Tile tile, int radius) {
            List<Tile> toRet = new List<Tile>();
            for (int i = -radius; i <= radius; i++) {
                int j = radius - Mathf.Abs(i);

                Tile tileAt = GetTileAt(tile.xPos + i, tile.yPos + j);
                if (tileAt != null) {
                    toRet.Add(tileAt);
                }
                if (j != 0) {
                    tileAt = GetTileAt(tile.xPos + i, tile.yPos - j);
                    if (tileAt != null) {
                        toRet.Add(tileAt);
                    }
                }
            }
            return toRet;
        }

        public Tile GetAccessibleAttackPosition(Tile source, Tile target) {
            IUnit u = source.GetPlaceable().GetGameObject().GetComponent<Unit>();
            List<Tile> listOfCandidateAttackTilePositions = GetCircumference(target, u.GetAttackRange());
            foreach (Tile t in listOfCandidateAttackTilePositions) {
                if (m_accessibleTiles.ContainsKey(t) && (t == source || m_accessibleTiles[t] == TileStateEnum.CanMove)) {
                    return t;
                }
            }
            return null;
        }

        public void DeterminePathableTiles(Tile tile, IUnit unit) {
            m_accessibleTiles.Clear();

            int max_range = unit.GetMaxMovement();
            int attack = unit.GetAttackRange();
            //Vector2 origin = new Vector2(tile.xPos, tile.yPos);

            List<Tile> TilesToFlip = new List<Tile>();
            FillTileAdjacency(tile.xPos, tile.yPos, max_range, attack, true);
            foreach (KeyValuePair<Tile, TileStateEnum> kvp in m_accessibleTiles) {
                //Debug.Log(string.Format("[DeterminePathableTiles] Tile({0}): {1}", kvp.Key, kvp.Value));
                switch (kvp.Value) {
                    case TileStateEnum.CanMove:
                        kvp.Key.sr.color = Color.blue;
                        break;
                    case TileStateEnum.CanAttack:
                        // actually an empty tile so it can't really "attack"
                        kvp.Key.sr.color = Color.red;
                        break;
                    case TileStateEnum.CanNotAccess:
                        kvp.Key.sr.color = Color.white;
                        if (tile != kvp.Key) {
                            List<Tile> listOfCandidateAttackTilePositions = GetCircumference(kvp.Key, attack);
                            //Debug.Log(string.Format("[DeterminePathableTiles] Test {1} locations if can attack Tile({0})", kvp.Key, listOfCandidateAttackTilePositions.Count));
                            foreach (Tile t in listOfCandidateAttackTilePositions) {
                                //Debug.Log(string.Format("[DeterminePathableTiles] t: ({0},{1})", t.xPos, t.yPos));
                                if (m_accessibleTiles.ContainsKey(t) && (t == tile || m_accessibleTiles[t] == TileStateEnum.CanMove)) {
                                    TilesToFlip.Add(kvp.Key);
                                    break;
                                }
                            }
                        }
                        break;
                }
            }

            //Debug.Log(string.Format("[DeterminePathableTiles] TilesToFlip: {0}", TilesToFlip.Count));
            for (int i = 0; i < TilesToFlip.Count; i++) {
                TilesToFlip[i].sr.color = Color.red;
                m_accessibleTiles[TilesToFlip[i]] = TileStateEnum.CanAttack;
            }
        }

        public TileStateEnum TileStateAt(Tile t) {
            if (m_accessibleTiles.ContainsKey(t)) {
                return m_accessibleTiles[t];
            }
            return TileStateEnum.CanNotAccess;
        }

        public void ClearPathableTiles() {
            foreach (KeyValuePair<Tile, TileStateEnum> kvp in m_accessibleTiles) {
                kvp.Key.sr.color = Color.white;
            }
        }
    }
}