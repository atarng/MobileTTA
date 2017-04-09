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

        public int Width {
            get {
                return m_width;
            }
            protected set {
                m_width = value;
            }
        }
        public int Height {
            get {
                return m_height;
            }
            protected set {
                m_height = value;
            }
        }

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

        public void InitializeGrid(int width = -1, int height = -1) {
            if (m_hasBeenInitialized) return;

            Width  = (width  > 0) ? width  : m_width;
            Height = (height > 0) ? height : m_height;

            float x_offset = (m_width / 2f) - .5f;
            float y_offset = (m_height / 2f) - .5f;

            m_grid = new List<Tile>();
            for (int i = 0; i < m_height; i++) { // row
                for (int j = 0; j < m_width; j++) { // col

                    Tile go = GameObject.Instantiate<Tile>(m_tilePrefab);

                    go.xPos = j;

                    go.yPos = i;

                    go.gameObject.name = "Tile_" + j + "_" + i;
                    //go.SetParentGrid(this);

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

        //         0,0
        //     0,0 0,1 0,0
        // 0,0 0,1 1,1 0,1 0,0
        // 0,0 0,1 2,1 1,1 0,1 0,0
        // 0,0 0,1 1,1 0,1 0,0
        //     0,0 0,1 0,0
        //         0,0
        private void FillTileAdjacency(int TileX, int TileY, int movesLeft, int attackRange, TileTraversalEnum canTraverse, int depth) {
            Tile tileAt = GetTileAt(TileX, TileY);
            if (tileAt == null) return;

            // self tile.
            if (depth == 0) {
                m_accessibleTiles.Add(tileAt, new TileState(TileStateEnum.CanStay, depth));
            }

            int deductedMoves = (depth == 0) ? movesLeft : movesLeft - tileAt.TraversalCost;
            if (movesLeft < 0 || (movesLeft > 0 && deductedMoves < 0)) {
                // was not legally allowed to enter this tile.
                return;
            }

            TileStateEnum stateToSet = (movesLeft > 0 && tileAt.CanTraverse(canTraverse)) ? TileStateEnum.CanMove : TileStateEnum.CanAttack;
            bool overrideDepth = false;

            if (tileAt.GetPlaceable() != null) {
                Unit u = tileAt.GetPlaceable() as Unit;
                if (u != null) {
                    if (!u.GetPlayerOwner().Equals(GameManager.GetInstance<GameManager>().CurrentPlayer())) {
                        stateToSet = TileStateEnum.Pending;
                    }
                    else {
                        stateToSet = TileStateEnum.CanPassThrough;
                    }
                }
                else {// atarng: for now this is an Impassable.
                    stateToSet = TileStateEnum.CanNotAccess;
                }
            }
            // if not added already, or if the depth is not as deep.
            if (m_accessibleTiles.ContainsKey(tileAt)) {
                overrideDepth = (depth < m_accessibleTiles[tileAt].Depth);
                overrideDepth = overrideDepth || (m_accessibleTiles[tileAt].Depth == depth && stateToSet == TileStateEnum.CanMove);
            }
            else {
                overrideDepth = true;
            }

            if (overrideDepth) {
                m_accessibleTiles[tileAt] = new TileState(stateToSet, depth);
            }

            // Debug.Log(string.Format("({0}, {1} :: {2}, {3}) : ({4}, {5})", 
            // TileX, TileY, movesLeft, attackRange, m_accessibleTiles[tileAt].TSE, m_accessibleTiles[tileAt].Depth));
            int tileOffset = 0;
            if (deductedMoves > 0) {
                tileOffset = 1;
            }
            else if (deductedMoves == 0) {
                tileOffset = attackRange;
                attackRange = 0;
            }
            bool tileAccessible = (m_accessibleTiles[tileAt].TSE == TileStateEnum.CanMove || m_accessibleTiles[tileAt].TSE == TileStateEnum.CanPassThrough);

            if (depth == 0 || (tileOffset > 0 && tileAccessible)) {
                // Left
                FillTileAdjacency(TileX - tileOffset, TileY, deductedMoves, attackRange, canTraverse, depth + 1);
                // UP
                FillTileAdjacency(TileX, TileY + tileOffset, deductedMoves, attackRange, canTraverse, depth + 1);
                // RIGHT
                FillTileAdjacency(TileX + tileOffset, TileY, deductedMoves, attackRange, canTraverse, depth + 1);
                // BOT
                FillTileAdjacency(TileX, TileY - tileOffset, deductedMoves, attackRange, canTraverse, depth + 1);

                // RANGED attacks
                //   .
                //  . .
                // . x .
                //  . .
                //   .
                if (deductedMoves < attackRange) {
                    for (int i = -attackRange; i <= attackRange; i++) {
                        int j = Mathf.Abs(i) - attackRange;
                        FillTileAdjacency(TileX + i, TileY + j, 0, 0, canTraverse, depth + 1);
                        FillTileAdjacency(TileX + i, TileY - j, 0, 0, canTraverse, depth + 1);
                    }
                }
            }
        }

        // GetCircumference(Tile tile, int radius)
        // tile: origin of the circle.
        // radius: tile Radius.
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
                if (m_accessibleTiles.ContainsKey(t) && 
                    (t == source || m_accessibleTiles[t].TSE == TileStateEnum.CanMove ||
                    m_accessibleTiles[t].TSE == TileStateEnum.CanStay ) ) {
                    retList.Add(t);
                }
            }
            return retList;
        }

        public void DeterminePathableTiles(Tile tile, IUnit unit) {
            m_accessibleTiles.Clear();

            int max_range = unit.GetMaxMovement();
            int atk_range = unit.GetAttackRange();
            //Vector2 origin = new Vector2(tile.xPos, tile.yPos);

            List<Tile> TilesToFlip = new List<Tile>();
            FillTileAdjacency(tile.xPos, tile.yPos, max_range, atk_range, unit.CanTraverse, 0);
            foreach (KeyValuePair<Tile, TileState> kvp in m_accessibleTiles) {
                switch (kvp.Value.TSE) {
                    case TileStateEnum.CanMove:
                        kvp.Key.Sprite.color = TileColors.BLUE;
                        break;
                    case TileStateEnum.CanAttack:
                        kvp.Key.Sprite.color = TileColors.RED;
                        break;
                    case TileStateEnum.CanPassThrough:
                        kvp.Key.Sprite.color = TileColors.GREY;
                        break;

                    case TileStateEnum.Pending:
                        kvp.Key.Sprite.color = TileColors.WHITE;
                        if (tile != kvp.Key) {
                            List<Tile> listOfCandidateAttackTilePositions = GetCircumference(kvp.Key, atk_range);
                            foreach (Tile t in listOfCandidateAttackTilePositions) {
                                if (m_accessibleTiles.ContainsKey(t) &&
                                   (t == tile || m_accessibleTiles[t].TSE == TileStateEnum.CanMove)) {
                                    Unit u = kvp.Key.GetPlaceable() as Unit;
                                    if (u != null && !u.GetPlayerOwner().Equals(GameManager.GetInstance<GameManager>().CurrentPlayer())) {
                                        TilesToFlip.Add(kvp.Key);
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    case TileStateEnum.CanNotAccess:
                    default:
                        kvp.Key.Sprite.color = TileColors.WHITE;
                        break;
                }
            }
            // Flip from pending to attack state.
            for (int i = 0; i < TilesToFlip.Count; i++) {
                TilesToFlip[i].Sprite.color = TileColors.RED;
                TileState ts = m_accessibleTiles[TilesToFlip[i]];
                //ts.TSE = TileStateEnum.CanAttack;
                m_accessibleTiles[TilesToFlip[i]] = new TileState(TileStateEnum.CanAttack, ts.Depth);
            }
        }

        public bool DisplaySummonableTiles(IGamePlayer p) {
            List<IUnit> units_summoned = p.GetCurrentSummonedUnits();
            HashSet<Tile> summonableTiles = new HashSet<Tile>();
            for (int i = 0; i < units_summoned.Count; i++) {
                List<Tile> t = GetCircumference(units_summoned[i].AssignedToTile, 1);
                for (int j = 0; j < t.Count; j++) {
                    if (t[j].GetPlaceable() == null && !summonableTiles.Contains(t[j]) ) {
                        summonableTiles.Add(t[j]);
                    }
                }
            }
            if (summonableTiles.Count > 0) {
                foreach (Tile t in summonableTiles) {
                    t.Sprite.color = TileColors.CYAN; //Color.cyan;
                }
            }
            // atarng: probably want to actually do this victory loss stuff somewhere else.
            else {
                return false;
            }
            return true;
        }

        public TileStateEnum TileStateAt(Tile t) {
            if (t != null) {
                if (m_accessibleTiles.ContainsKey(t)) {
                    return m_accessibleTiles[t].TSE;
                }
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
                m_grid[i].Sprite.color = TileColors.WHITE;
            }
        }



    }
}