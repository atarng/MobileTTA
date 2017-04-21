using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using AtRng.MobileTTA;

public class GameManager : SceneControl, ISoundManager {
    /*** MAP INIT ***/

    public LevelScriptableObject LevelInitData;

    /*** MAP INIT ***/
    [SerializeField]
    private List<Transform> playerLocations;

    [SerializeField]
    private Player   m_playerPrefab;
    [SerializeField]
    private AIPlayer m_aiPlayerPrefab;

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
    public Transform m_militiaButton;

    Queue<BasePlayer> m_turnQueue = new Queue<BasePlayer>();
    Dictionary<int, BasePlayer> m_idPlayerMap = new Dictionary<int, BasePlayer>();

    // TEMPPPP
    public GameUnit   m_unitPrefab;
    public AIUnit     m_AIUnitPrefab;
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
        BasePlayer bp = null;
        for (int i = 0; i < playerLocations.Count; i++) { //m_number_of_players;
            if ( i > 0 && LevelInitData.UsesAIOpponent) {
                bp = GameObject.Instantiate<AIPlayer>(m_aiPlayerPrefab);
            }
            else {
                bp = GameObject.Instantiate<Player>(m_playerPrefab);
            }

            bp.transform.SetParent(transform);
            bp.transform.position = CameraManager.Instance.FromUIToGameVector(playerLocations[i].position);

            Vector3 pPos = bp.transform.position;
            pPos.z = 0;
            bp.transform.position = pPos;
            bp.transform.localRotation = Quaternion.Euler(0, 0, (i * 180));
            bp.ID = i;

            // Instantiate Player Deck.
            if ( i == 0 && LevelInitData.UsesPlayerDeckList ) {
                List<UnitManager.UnitPersistence> playerDeck = 
                    SaveGameManager.GetSaveGameData().LoadFrom("TestDeck") as List<UnitManager.UnitPersistence>;
                if (playerDeck != null) {
                    Player p = bp as Player;
                    if(p != null) {
                        p.PopulateAndShuffleDeck<UnitManager.UnitPersistence>(playerDeck);
                    }
                }
            }

            if (!m_idPlayerMap.ContainsKey(i)) {
                m_idPlayerMap.Add(i, bp);
            }
            else {
                Debug.LogError("Inserting Duplicate Player Key: " + i);
            }
            m_turnQueue.Enqueue(bp);
        }

        m_turnQueue.Peek().BeginTurn();
    }

    private void MapInit() {
        //Debug.Log("[GameManager/MapInit] LevelInitData: " + LevelInitData.name);

        m_gridInstance.InitializeGrid(LevelInitData.Width, LevelInitData.Height);

        // Placeables Array
        for (int i = 0; i < LevelInitData.PlaceablesArray.Length; i++) {
            Tile tileAtXY = m_gridInstance.GetTileAt((LevelInitData.PlaceablesArray[i].Y), (LevelInitData.PlaceablesArray[i].X));

            switch (LevelInitData.PlaceablesArray[i].placeableType) {
                case PlaceableType.Unit:
                    // Create Unit
                    GameUnit unit_to_place_on_tile = GameObject.Instantiate<GameUnit>(m_unitPrefab);
                    UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(LevelInitData.PlaceablesArray[i].ID);
                    unit_to_place_on_tile.ReadDefinition(ud);
                    unit_to_place_on_tile.transform.localScale = Vector3.one * .01f;

                    // Assign To Player
                    IGamePlayer p = SingletonMB.GetInstance<GameManager>().GetPlayer(LevelInitData.PlaceablesArray[i].PlayerID);
                    if (unit_to_place_on_tile.IsNexus()) {
                        p.PlaceUnitOnField(unit_to_place_on_tile);
                    }
                    else {
                        p.GetCurrentSummonedUnits().Add(unit_to_place_on_tile);
                    }

                    // Assign to Tile.
                    tileAtXY.SetPlaceable(unit_to_place_on_tile);

                    IUnit iUnit = unit_to_place_on_tile;
                    iUnit.AssignPlayerOwner(LevelInitData.PlaceablesArray[i].PlayerID);
                    iUnit.AssignedToTile = tileAtXY;

                    break;
                case PlaceableType.Impassable:
                    Impassable impassable = GameObject.Instantiate<Impassable>(m_impassable);
                    tileAtXY.SetPlaceable(impassable);
                    break;
                case PlaceableType.Tile:
                    tileAtXY.SetTileTraversal((TileTraversalEnum)LevelInitData.PlaceablesArray[i].ID);
                    break;
            }
        }

        // Populate Opposing Player Deck.
        BasePlayer opposingPlayer = SingletonMB.GetInstance<GameManager>().GetPlayer(1);
        if (opposingPlayer != null) {
            if (LevelInitData.OpposingDeckList != null && LevelInitData.OpposingDeckList.Length > 0) {
                opposingPlayer.PopulateAndShuffleDeck(LevelInitData.OpposingDeckList.ToList());
            }
            else if (LevelInitData.UsesOpponentDeckList) {
                opposingPlayer.PopulateAndShuffleDeck<UnitManager.UnitDesciption>(InitializeDummyDeck_Temp());
            }
        }
        else {
            Debug.LogError("[GameManager/MapInit] Could not grab opposing player");
        }
    }

    public BasePlayer GetPlayer(int id) {
        if (!b_initialized) {
            Start();
        }
        if (m_idPlayerMap.ContainsKey(id)) {
            return m_idPlayerMap[id];
        }
        return null;
    }

    public BasePlayer CurrentPlayer() {
        return m_turnQueue.Peek();
    }

    public void UI_EndTurn() {
        BasePlayer p = m_turnQueue.Peek();
        p.EndTurn();
    }

        
    private bool CheckVictory(BasePlayer winningPlayer, BasePlayer losingPlayer) {
        bool hasWon = losingPlayer.CheckIfLost();

        /*
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
        */
        // player is surrounded with no units left.
        // hasWon |= !(GetGrid().DisplaySummonableTiles(losingPlayer));
        // GetGrid().ClearPathableTiles();

        if (hasWon) {
            if (winningPlayer.ID == 0) {
                // Award Currency.
            }
            string toDisplay = string.Format("Player{0} has Won!", winningPlayer.ID);
            SceneControl.GetCurrentSceneControl().DisplayInfo(toDisplay);
            WaitThenExecute.CreateWaitForSecondsThanExecute( 2, () => {
                const string MAPS_NAME = "MapSelector";
                SceneManager.LoadScene(MAPS_NAME);
            });
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


        m_militiaButton.localRotation = Quaternion.Euler(0, 0, (currentPlayer.ID * 180));

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
    public void HandleCombat(ICombat combatant1, ICombat combatant2) {
        IUnit iu1 = combatant1 as IUnit;
        IUnit iu2 = combatant2 as IUnit;

        int iu1p_damageToDo = 0;
        int iu1s_damageToDo = 0;
        int iu2p_damageToDo = 0;
        int iu2s_damageToDo = 0;

        int dist = Mathf.Abs(combatant1.AssignedToTile.xPos - combatant2.AssignedToTile.xPos) +
                   Mathf.Abs(combatant1.AssignedToTile.yPos - combatant2.AssignedToTile.yPos);

        if (iu1 != null && iu1.GetAttackRange() == dist) {
            iu1p_damageToDo = iu1.IsPhysicalAttack()  ? iu1.GetAttackValue() : 0;
            iu1s_damageToDo = iu1.IsSpiritualAttack() ? iu1.GetAttackValue() : 0;
        }
        if (iu2 != null && iu2.GetAttackRange() == dist) {
            iu2p_damageToDo = iu2.IsPhysicalAttack()  ? iu2.GetAttackValue() : 0;
            iu2s_damageToDo = iu2.IsSpiritualAttack() ? iu2.GetAttackValue() : 0;
        }

        combatant2.TakeDamage(iu1p_damageToDo, iu1s_damageToDo);

        if (m_combatOrder && combatant2.IsAlive() ) {
            combatant1.TakeDamage(iu2p_damageToDo, iu2s_damageToDo);
        }

        CheckVictory(GetPlayer(0), GetPlayer(1));
        CheckVictory(GetPlayer(1), GetPlayer(0));
    }

    [SerializeField]
    AudioClip[] m_tempSoundArray;
    [SerializeField]
    AudioSource m_tempAudioSource;
    public void PlaySound(string soundKey) {
        switch (soundKey) {
            case "Tile":
                m_tempAudioSource.clip = m_tempSoundArray[0];
                break;
            case "Draw":
                m_tempAudioSource.clip = m_tempSoundArray[1];
                break;
        }
        m_tempAudioSource.Play();
    }

    ///*
    //*/
}