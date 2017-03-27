using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using AtRng.MobileTTA;

public class GameManager : SceneControl {
    /*** MAP INIT ***/

    public LevelScriptableObject LevelInitData;

    /*** MAP INIT ***/
    [SerializeField]
    private List<Transform> playerLocations;

    [SerializeField]
    private Player m_playerPrefab;
    [SerializeField]
    private SpriteRenderer m_actionPointPrefab;
    public SpriteRenderer GetActionPointSprite(){
        return m_actionPointPrefab;
    }

    [SerializeField]
    private Grid m_gridInstance;
    public Grid GetGrid() {
        return m_gridInstance;
    }

    public Transform m_playerIndicator;

    Queue<BasePlayer> m_turnQueue = new Queue<BasePlayer>();
    Dictionary<int, BasePlayer> m_idPlayerMap = new Dictionary<int, BasePlayer>();

    // TEMPPPP
    public Unit       m_unitPrefab;
    public Impassable m_impassable;

    bool b_initialized = false;

    public int[] m_testDeckList;
    //private List<UnitManager.UnitDefinition> to_insert = new List<UnitManager.UnitDefinition>();

    /*** DEBUG/CONTROLS ***/
    bool m_debug_mouse;
    bool m_drawMode = false;
    bool m_combatOrder = true;
    public void ToggleDrawMode() {
        m_drawMode = !m_drawMode;
    }
    public bool DrawMode() {
        return m_drawMode;
    }
    public void ToggleDebugMouse() {
        m_debug_mouse = !m_debug_mouse;
    }
    public void ToggleCombatOrder() {
        m_combatOrder = !m_combatOrder;
    }
    /*** DEBUG/CONTROLS ***/

    private void Start() {
        if (GamePlayNavigation.loadedLevel != null) {
            LevelInitData = GamePlayNavigation.loadedLevel;
        }

        if (!b_initialized) {
            b_initialized = true;
            InitializePlayers();
        }
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

            if ( i == 0 && LevelInitData.UsesPlayerDeckList ) {
                List<UnitManager.UnitPersistence> playerDeck = 
                    SaveGameManager.GetSaveGameData().LoadFrom("TestDeck") as List<UnitManager.UnitPersistence>;
                if (playerDeck != null) {
                    p.PopulateAndShuffleDeck<UnitManager.UnitPersistence>(playerDeck);
                }
            }

            if (!m_idPlayerMap.ContainsKey(i)) {
                m_idPlayerMap.Add(i, p);
            }
            else {
                Debug.LogError("Inserting Duplicate Player Key: " + i);
            }
            m_turnQueue.Enqueue(p);
        }

        m_turnQueue.Peek().BeginTurn();
    }

    private void MapInit() {
        //Debug.Log("[GameManager/MapInit] LevelInitData: " + LevelInitData.name);

        m_gridInstance.InitializeGrid(LevelInitData.Width, LevelInitData.Height);

        // Placeables Array
        for (int i = 0; i < LevelInitData.PlaceablesArray.Length; i++) {
            switch (LevelInitData.PlaceablesArray[i].placeableType) {
                case PlaceableType.Unit: {
                    // Create Unit
                    Unit unit_to_place_on_tile = GameObject.Instantiate<Unit>(m_unitPrefab);
                    UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(LevelInitData.PlaceablesArray[i].ID);
                    unit_to_place_on_tile.ReadDefinition(ud);

                    // Assign To Player
                    IGamePlayer p = SingletonMB.GetInstance<GameManager>().GetPlayer(LevelInitData.PlaceablesArray[i].PlayerID);
                    p.GetCurrentSummonedUnits().Add(unit_to_place_on_tile);

                    // Assign to Tile.
                    Tile tileAtXY = m_gridInstance.GetTileAt((LevelInitData.PlaceablesArray[i].X), (LevelInitData.PlaceablesArray[i].Y));
                    tileAtXY.SetPlaceable(unit_to_place_on_tile);

                    IUnit iUnit = unit_to_place_on_tile;
                    iUnit.AssignPlayerOwner(LevelInitData.PlaceablesArray[i].PlayerID);
                    iUnit.AssignedToTile = tileAtXY;

                    break;
                }
                case PlaceableType.Impassable: {
                    Impassable impassable = GameObject.Instantiate<Impassable>(m_impassable);

                    Tile tileAtXY = m_gridInstance.GetTileAt((LevelInitData.PlaceablesArray[i].X), (LevelInitData.PlaceablesArray[i].Y));
                    tileAtXY.SetPlaceable(impassable);

                    break;
                }
            }
        }

        Player opposingPlayer = SingletonMB.GetInstance<GameManager>().GetPlayer(1) as Player;
        if(opposingPlayer != null) {
            if (LevelInitData.OpposingDeckList != null && LevelInitData.OpposingDeckList.Length > 0) {
                opposingPlayer.PopulateAndShuffleDeck(LevelInitData.OpposingDeckList.ToList());
            }
            else {
                opposingPlayer.PopulateAndShuffleDeck<UnitManager.UnitDesciption>(InitializeDummyDeck_Temp());
            }
        }

    }

    public BasePlayer GetPlayer(int id) {
        if (!b_initialized) {
            Start();
        }
        return m_idPlayerMap[id];
    }

    public BasePlayer CurrentPlayer() {
        return m_turnQueue.Peek();
    }

    public void UI_EndTurn() {
        BasePlayer p = m_turnQueue.Peek();
        p.EndTurn();
    }

        
    private bool CheckVictory(BasePlayer winningPlayer, BasePlayer losingPlayer) {
        bool hasWon = false;
        if (losingPlayer.HandSize() == 0 && losingPlayer.DeckSize() == 0) {
            if (losingPlayer.GetCurrentSummonedUnits().Count == 0) {
                hasWon = true;
            }
            else if (losingPlayer.GetCurrentSummonedUnits().Count == 1) {
                Unit u = losingPlayer.GetCurrentSummonedUnits()[0] as Unit;
                if (u.IsNexus()) {
                    hasWon = true;
                }
            }
        }
        // player is surrounded
        hasWon |= !(GetGrid().DisplaySummonableTiles(losingPlayer));
        GetGrid().ClearPathableTiles();

        if (hasWon) {
            string toDisplay = string.Format("Player{0} has Won!", winningPlayer.ID);

            if (winningPlayer.ID == 0) {
                // Award Currency.
            }

            SceneControl.GetCurrentSceneControl().DisplayInfo(toDisplay);
        }

        return hasWon;
    }

    public void UpdateTurn() {

        BasePlayer p = m_turnQueue.Dequeue();
        m_turnQueue.Enqueue(p);

        BasePlayer currentPlayer = m_turnQueue.Peek();
        currentPlayer.BeginTurn();

        if (m_drawMode) {
            currentPlayer.Draw();
        }

        Vector3 newScale = m_playerIndicator.localScale;
        newScale.y = newScale.y > 0 ? -1 : 1;
        m_playerIndicator.localScale = newScale;

        CheckVictory(p, currentPlayer);
    }
    
    protected override void Update() {
        base.Update();

        if (m_debug_mouse) {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
                RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).x,
                                          CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).y),
                                          Vector2.zero, 0f);
                if (rh2d) {
                    SceneControl.GetCurrentSceneControl().DisplayInfo("RayCastHit: " + rh2d.transform.name);
                }
            }
        }
    }

    /***
     * Combat Order
     */
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

        combatant2.TakeDamage(iu1p_damageToDo, 0);
        combatant2.TakeDamage(iu1s_damageToDo, 1);

        if (m_combatOrder && combatant2.IsAlive() ) {
            combatant1.TakeDamage(iu2p_damageToDo, 0);
            combatant1.TakeDamage(iu2s_damageToDo, 1);
        }

        CheckVictory(GetPlayer(0), GetPlayer(1));
        CheckVictory(GetPlayer(1), GetPlayer(0));
    }
    ///*
    //*/
}