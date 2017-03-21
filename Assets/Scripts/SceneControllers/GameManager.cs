using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AtRng.MobileTTA;

public class GameManager : SceneControl {
    /*
    [Serializable]
    struct PlayerInit {
        public Transform UILocation;
        public Vector2 nexusLocation;
    }
    */
    /*** MAP INIT ***/
    [Serializable]
    struct IntVector2 {
        public int x;
        public int y;
        public int Player;
        public int unitType;
    }

    [SerializeField]
    List<IntVector2> m_tilesToInitUnits;
    /*** MAP INIT ***/
    [SerializeField]
    private List<Transform> playerLocations;

    [SerializeField]
    private Player m_playerPrefab;

    [SerializeField]
    private Grid m_gridInstance;
    public Grid GetGrid() {
        return m_gridInstance;
    }

    Queue<Player> m_turnQueue = new Queue<Player>();
    Dictionary<int, Player> m_idPlayerMap = new Dictionary<int, Player>();

    // TEMPPPP
    public Unit m_unitPrefab;

    bool b_initialized = false;
    bool load_deck_from_file = false;

    public int[] m_testDeckList;
    //private List<UnitManager.UnitDefinition> to_insert = new List<UnitManager.UnitDefinition>();

    private void Start() {
        if (!b_initialized) {
            InitializePlayers();
            
        }
        b_initialized = true;

        MapInit();
    }

    private List<UnitManager.UnitDesciption> InitializeDummyDeck_Temp() {
        List<UnitManager.UnitDesciption> to_insert = new List<UnitManager.UnitDesciption>();

        for (int i = 0; i < m_testDeckList.Length; i++) {
            UnitManager um = SingletonMB.GetInstance<UnitManager>();
            UnitManager.UnitDefinition ud = um.GetDefinition(m_testDeckList[i]);
            if (ud != null) {
                to_insert.Add(ud);
            }
        }

        return to_insert;
    }

    public void InitializePlayers() {// List<UnitManager.UnitDefinition> deckList) {
        for (int i = 0; i < playerLocations.Count; i++) { //m_number_of_players;
            Player p = GameObject.Instantiate<Player>(m_playerPrefab);
            p.transform.SetParent(transform);
            p.transform.position = CameraManager.Instance.FromUIToGameVector( playerLocations[i].position );//new Vector3(i, 0.5f, 10) );

            Vector3 pPos = p.transform.position;
            pPos.z = 0;
            p.transform.position = pPos;
            p.transform.localRotation = Quaternion.Euler(0, 0, (i * 180));
            p.ID = i;

            if (i == 0) {
                List<UnitManager.UnitPersistence> playerDeck = 
                    SaveGameManager.GetSaveGameData().LoadFrom("TestDeck") as List<UnitManager.UnitPersistence>;
                if (playerDeck != null) {
                    p.PopulateAndShuffleDeck<UnitManager.UnitPersistence>(playerDeck);
                }
            }
            else {
                p.PopulateAndShuffleDeck<UnitManager.UnitDesciption>(InitializeDummyDeck_Temp());
            }

            m_idPlayerMap.Add(i, p);
            m_turnQueue.Enqueue(p);
        }

        m_turnQueue.Peek().Reset();
    }


    private void MapInit() {
        m_gridInstance.InitializeGrid();
        for (int i = 0; i < m_tilesToInitUnits.Count; i++) {

            // NEXUS
            Unit unit_to_place_on_tile = GameObject.Instantiate<Unit>(m_unitPrefab);
            UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(m_tilesToInitUnits[i].unitType);
            unit_to_place_on_tile.ReadDefinition(ud);


            // this creates a dependency on GameManager.
            IGamePlayer p = GameManager.GetInstance<GameManager>().GetPlayer(m_tilesToInitUnits[i].Player);
            p.GetCurrentSummonedUnits().Add(unit_to_place_on_tile);

            Tile tileAtXY = m_gridInstance.GetTileAt((m_tilesToInitUnits[i].x), (m_tilesToInitUnits[i].y));
            tileAtXY.SetPlaceable(unit_to_place_on_tile);

            IUnit iUnit = unit_to_place_on_tile;
            iUnit.AssignPlayerOwner(m_tilesToInitUnits[i].Player);
            iUnit.AssignedToTile = tileAtXY;
        }
    }

    public IGamePlayer GetPlayer(int id) {
        if (!b_initialized) {
            Start();
        }
        return m_idPlayerMap[id];
    }

    public Player CurrentPlayer() {
        return m_turnQueue.Peek();
    }

    public void UI_EndTurn() {
        Player p = m_turnQueue.Peek();
        p.EndTurn();
    }

    public void UpdateTurn() {
        Player p = m_turnQueue.Dequeue();
        m_turnQueue.Enqueue(p);

        m_turnQueue.Peek().Reset();
    }

    public void HandleCombat(ICombatPlaceable combatant1, ICombatPlaceable combatant2) {
        IUnit iu1 = combatant1 as IUnit;
        IUnit iu2 = combatant2 as IUnit;
        int iu1p_damageToDo = 0;
        int iu1s_damageToDo = 0;
        int iu2p_damageToDo = 0;
        int iu2s_damageToDo = 0;

        int dist = Mathf.Abs(iu1.AssignedToTile.xPos - iu2.AssignedToTile.xPos) +
                   Mathf.Abs(iu1.AssignedToTile.yPos - iu2.AssignedToTile.yPos);

        if (iu1 != null && iu1.GetAttackRange() == dist) {
            iu1p_damageToDo = iu1.IsPhysicalAttack()  ? iu1.GetAttackValue() : 0;
            iu1s_damageToDo = iu1.IsSpiritualAttack() ? iu1.GetAttackValue() : 0;
        }
        if (iu2 != null && iu2.GetAttackRange() == dist) {
            iu2p_damageToDo = iu2.IsPhysicalAttack()  ? iu2.GetAttackValue() : 0;
            iu2s_damageToDo = iu2.IsSpiritualAttack() ? iu2.GetAttackValue() : 0;
        }
        combatant1.TakeDamage(iu2p_damageToDo, 0);
        combatant1.TakeDamage(iu2s_damageToDo, 1);

        combatant2.TakeDamage(iu1p_damageToDo, 0);
        combatant2.TakeDamage(iu1s_damageToDo, 1);

    }
    ///*
    //*/
}