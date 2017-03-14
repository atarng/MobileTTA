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

        struct TileState {
            public TileStateEnum TSE { get; private set; }
            public int Depth { get; private set; }
            public TileState(TileStateEnum tse, int depth) {
                TSE = tse;
                Depth = depth;
            }
        }
        private Dictionary<Tile, TileState> m_accessibleTiles = new Dictionary<Tile, TileState>();

        void Awake() {}
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

        private void FillTileAdjacency(int TileX, int TileY, int MovesLeft, int attackRange, int depth) {
            Tile tileAt = GetTileAt(TileX, TileY);
            if (tileAt == null) return;
            if (depth == 0) {
                //Debug.Log(string.Format("FillTileAdjacency: ({0}, {1})", TileX, TileY));
                m_accessibleTiles.Add(tileAt, new TileState( TileStateEnum.CanNotAccess, depth ) );
            }

            int dm = 1;
            int da = 0;

            if (MovesLeft == 0) {
                if (attackRange > 0) {
                    dm = 0;
                    da = attackRange;
                }
                else {
                    // add if not already in list?
                    if (!m_accessibleTiles.ContainsKey(tileAt)) {
                        if (tileAt.GetPlaceable() == null) {
                            // Just for visual effect. Empty Tile so can't actually attack.
                            //m_accessibleTiles.Add(tileAt, TileStateEnum.CanAttack);
                            m_accessibleTiles.Add(tileAt, new TileState(TileStateEnum.CanAttack, depth));
                            
                        }
                        else {
                            // Possibly need to change this to can attack if there is a pathable location to attack this object.
                            //m_accessibleTiles.Add(tileAt, TileStateEnum.CanNotAccess);
                            TileStateEnum stateToSet = TileStateEnum.CanMove;
                            if (tileAt.GetPlaceable() != null) {
                                Unit u = tileAt.GetPlaceable() as Unit;
                                if (u != null) {
                                    if (!u.GetPlayerOwner().Equals(GameManager.GetInstance<GameManager>().CurrentPlayer())) {
                                        stateToSet = TileStateEnum.CanNotAccess;
                                    }
                                    else {
                                        stateToSet = TileStateEnum.CanPassThrough;
                                    }
                                }
                            }
                            m_accessibleTiles.Add(tileAt, new TileState(stateToSet, depth));
                        }
                    }
                    return;
                }
            }

            if (depth > 0 && (MovesLeft > 0 || attackRange > 0)) {
                // If not the initial call, and there are still moves left or attack range left.
                // Check current Tile to see if it is occupied.
                // If it is occupied, it is either something that can be passed through (owned by current player)
                // or it is something that might be able to be attacked.

                TileStateEnum stateToSet = TileStateEnum.CanMove;
                if (tileAt.GetPlaceable() != null) {
                    Unit u = tileAt.GetPlaceable() as Unit;
                    if (u != null) {
                        if (!u.GetPlayerOwner().Equals(GameManager.GetInstance<GameManager>().CurrentPlayer())) {
                            stateToSet = TileStateEnum.CanNotAccess;
                        }
                        else {
                            stateToSet = TileStateEnum.CanPassThrough;
                        }
                    }
                }

                if (!m_accessibleTiles.ContainsKey(tileAt)) {
                    m_accessibleTiles.Add(tileAt, new TileState(stateToSet, depth) );
                }
                // TODO: REVISIT THIS LOGIC AND CORRECT IT.
                else if (m_accessibleTiles[tileAt].TSE == TileStateEnum.CanAttack) {
                    // || m_accessibleTiles[tileAt] == TileStateEnum.CanNotAccess) {
                    m_accessibleTiles[tileAt] = new TileState(stateToSet, depth); //stateToSet;
                }
            }

            if (depth == 0 || m_accessibleTiles.ContainsKey(tileAt)
                && (m_accessibleTiles[tileAt].TSE == TileStateEnum.CanMove || m_accessibleTiles[tileAt].TSE == TileStateEnum.CanPassThrough)
            ) {
                // LEFT
                FillTileAdjacency(TileX - dm - da, TileY, MovesLeft - dm, attackRange - da, depth + 1);
                // UP
                FillTileAdjacency(TileX, TileY + dm + da, MovesLeft - dm, attackRange - da, depth + 1);
                // RIGHT
                FillTileAdjacency(TileX + dm + da, TileY, MovesLeft - dm, attackRange - da, depth + 1);
                // BOT
                FillTileAdjacency(TileX, TileY - dm - da, MovesLeft - dm, attackRange - da, depth + 1);

                // RANGED attacks
                if (MovesLeft < attackRange) {
                    for (int i = -attackRange; i <= attackRange; i++) {
                        int j = Mathf.Abs(i) - attackRange;
                        FillTileAdjacency(TileX + i, TileY + j, 0, 0, depth + 1);
                        FillTileAdjacency(TileX + i, TileY - j, 0, 0, depth + 1);
                    }
                }

                // L
                //if(MovesLeft < attackRange) {
                //    FillTileAdjacency(TileX - attackRange, TileY, 0, 0);
                //}
                // U
                //if (MovesLeft < attackRange) {
                //    FillTileAdjacency(TileX, TileY + attackRange, 0, 0);
                //}
                // R
                //if (MovesLeft < attackRange) {
                //    FillTileAdjacency(TileX + attackRange, TileY, 0, 0);
                //}
                // B
                //if (MovesLeft < attackRange) {
                //    FillTileAdjacency(TileX, TileY - attackRange, 0, 0);
                //}
            }
        }

        // (-1, 0), (0, 1), (0, -1), (1,0)
        public List<Tile> GetCircumference(Tile tile, int radius) {
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

        public List<Tile> GetAccessibleAttackPositions(Tile source, Tile target) {
            IUnit u = source.GetPlaceable().GetGameObject().GetComponent<Unit>();
            List<Tile> listOfCandidateAttackTilePositions = GetCircumference(target, u.GetAttackRange());
            List<Tile> retList = new List<Tile>();
            foreach (Tile t in listOfCandidateAttackTilePositions) {
                if (m_accessibleTiles.ContainsKey(t) && (t == source || m_accessibleTiles[t].TSE == TileStateEnum.CanMove)) {
                    retList.Add(t);
                }
            }
            return retList;
        }

        public void DeterminePathableTiles(Tile tile, IUnit unit) {
            m_accessibleTiles.Clear();

            int max_range = unit.GetMaxMovement();
            int attack = unit.GetAttackRange();
            //Vector2 origin = new Vector2(tile.xPos, tile.yPos);

            List<Tile> TilesToFlip = new List<Tile>();
            FillTileAdjacency(tile.xPos, tile.yPos, max_range, attack, 0);
            foreach (KeyValuePair<Tile, TileState> kvp in m_accessibleTiles) {
                switch (kvp.Value.TSE) {
                    case TileStateEnum.CanMove:
                        kvp.Key.sr.color = Color.blue;
                        break;
                    case TileStateEnum.CanAttack:
                        kvp.Key.sr.color = Color.red;
                        break;
                    case TileStateEnum.CanPassThrough:
                        kvp.Key.sr.color = Color.gray;
                        break;
                    case TileStateEnum.CanNotAccess:
                        kvp.Key.sr.color = Color.white;
                        if (tile != kvp.Key) {
                            List<Tile> listOfCandidateAttackTilePositions = GetCircumference(kvp.Key, attack);
                            foreach (Tile t in listOfCandidateAttackTilePositions) {
                                if (m_accessibleTiles.ContainsKey(t) &&
                                   (t == tile || m_accessibleTiles[t].TSE == TileStateEnum.CanMove))
                                {
                                    Unit u = kvp.Key.GetPlaceable() as Unit;
                                    if (u != null && !u.GetPlayerOwner().Equals(GameManager.GetInstance<GameManager>().CurrentPlayer())) {
                                        TilesToFlip.Add(kvp.Key);
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            for (int i = 0; i < TilesToFlip.Count; i++) {
                TilesToFlip[i].sr.color = Color.red;
                TileState ts = m_accessibleTiles[TilesToFlip[i]];
                //ts.TSE = TileStateEnum.CanAttack;
                m_accessibleTiles[TilesToFlip[i]] = new TileState(TileStateEnum.CanAttack, ts.Depth);
            }
        }

        public void DisplaySummonableTiles(IGamePlayer p) {
            List<IUnit> units_summoned = p.GetCurrentSummonedUnits();
            HashSet<Tile> hs_tile = new HashSet<Tile>();
            for (int i = 0; i < units_summoned.Count; i++) {
                List<Tile> t = GetCircumference(units_summoned[i].AssignedToTile, 1);
                for (int j = 0; j < t.Count; j++) {
                    if (t[j].GetPlaceable() == null && !hs_tile.Contains(t[j]) ) {
                        hs_tile.Add(t[j]);
                    }
                }
            }

            foreach (Tile t in hs_tile) {
                t.sr.color = Color.cyan;
            }
        }

        public TileStateEnum TileStateAt(Tile t) {
            if (t != null) {
                if (m_accessibleTiles.ContainsKey(t)) {
                    return m_accessibleTiles[t].TSE;
                }
            }
            else {
                Debug.LogWarning("[Grid/TileStateAt] Tile t key is Null.");
            }
            return TileStateEnum.CanNotAccess;
        }

        public void ClearPathableTiles() {
            /*
            foreach (KeyValuePair<Tile, TileStateEnum> kvp in m_accessibleTiles) {
                kvp.Key.sr.color = Color.white;
            }
            */
            for (int i = 0; i < m_grid.Count; i++) {
                m_grid[i].sr.color = Color.white;
            }
        }
    }
}