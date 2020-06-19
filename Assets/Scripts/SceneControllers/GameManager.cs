using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using AtRng.MobileTTA;
//, ISoundManager
public class GameManager : SceneControl {
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
    private ActionPoint_MB m_actionPointPrefab;
    public ActionPoint_MB GetActionPointSprite(){
        return m_actionPointPrefab;
    }

    [SerializeField]
    private AtRng.MobileTTA.Grid m_gridInstance;
    public AtRng.MobileTTA.Grid GetGrid() {
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

    [SerializeField]
    AudioClip[] m_tempSoundArray;
    [SerializeField]
    AudioSource m_tempAudioSource;

    Dictionary<string, Action> m_eventListeners;

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

        m_eventListeners = new Dictionary<string, Action>();

        MapInit();
    }

    public void RegisterEventListener(string event_name, Action a) {
        Action toAddTo = null;
        if (m_eventListeners.ContainsKey(event_name)) {
            toAddTo = m_eventListeners[event_name];
        }
        else {
            toAddTo = () => { };
        }
        toAddTo += a;
        m_eventListeners[event_name] = toAddTo;
    }

    public void BroadcastEvent(string event_name) {
        if (m_eventListeners.ContainsKey(event_name)) {
            if(m_eventListeners[event_name] != null) {
                m_eventListeners[event_name]();
            }
        }
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

            bp.transform.SetParent(playerLocations[i]);
            //bp.transform.position = CameraManager.Instance.FromUIToGameVector(playerLocations[i].position);
            //Vector3 pPos = bp.transform.position;
            //pPos.z = 0;
            bp.transform.position = Vector3.zero;

            bp.transform.localRotation = Quaternion.Euler(0, 0, (i * 180));
            bp.transform.localScale = Vector3.one;
            //bp.transform.localScale = (i % 2 > 0) ? (Vector3.one * -1) : Vector3.one;
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
                    //
                    int playerID = LevelInitData.PlaceablesArray[i].PlayerID;
                    BaseUnit toInstantiate = (playerID == 0 || !LevelInitData.UsesAIOpponent) ? (BaseUnit)m_unitPrefab : (BaseUnit)m_AIUnitPrefab;
                    BaseUnit unit_to_place_on_tile = GameObject.Instantiate(toInstantiate);
                    UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(LevelInitData.PlaceablesArray[i].ID);

                    IUnit iUnit = unit_to_place_on_tile;
                    iUnit.AssignedToTile = tileAtXY;
                    iUnit.AssignPlayerOwner(LevelInitData.PlaceablesArray[i].PlayerID);

                    unit_to_place_on_tile.ReadDefinition(ud);
                    unit_to_place_on_tile.transform.localScale = Vector3.one * .01f;
                    //unit_to_place_on_tile.transform.localRotation = Quaternion.Euler(0, 0, (playerID * 180));

                    // Assign To Player
                    IGamePlayer p = SingletonMB.GetInstance<GameManager>().GetPlayer(playerID);
                    if (unit_to_place_on_tile.IsNexus()) {
                        p.PlaceUnitOnField(unit_to_place_on_tile);
                    }
                    else {
                        p.GetCurrentSummonedUnits().Add(unit_to_place_on_tile);
                    }

                    // Assign to Tile.
                    tileAtXY.SetPlaceable(unit_to_place_on_tile);

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

        // Load Tutorial Script:
        if(LevelInitData.TutorialPrefab) {
            GameObject.Instantiate(LevelInitData.TutorialPrefab);
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
        if (p is Player) {
            p.EndTurn();
        }
    }

        
    private bool CheckVictory(BasePlayer winningPlayer, BasePlayer losingPlayer) {
        bool hasWon = losingPlayer.CheckIfLost();
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
        AudioManager sound_manager_temp = SingletonMB.GetInstance<AudioManager>();
        sound_manager_temp.PlaySound("NewTurn");

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


        //m_militiaButton.localRotation = Quaternion.Euler(0, 0, (currentPlayer.ID * 180));
        m_militiaButton.localScale = (currentPlayer.ID > 0) ? -(Vector3.one) : Vector3.one;

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
        AudioManager sound_manager_temp = SingletonMB.GetInstance<AudioManager>();
        sound_manager_temp.PlaySound("Combat1");

        CheckVictory(GetPlayer(0), GetPlayer(1));
        CheckVictory(GetPlayer(1), GetPlayer(0));

    }

    //public void PlaySound(string soundKey) {
    //    if (!m_tempAudioSource.isPlaying) {
    //        switch (soundKey) {
    //            case "Tile":
    //                m_tempAudioSource.clip = m_tempSoundArray[0];
    //                break;
    //            case "Draw":
    //                m_tempAudioSource.clip = m_tempSoundArray[1];
    //                break;
    //            case "Combat1":
    //            case "Combat2":
    //                m_tempAudioSource.clip = m_tempSoundArray[2];
    //                break;
    //            case "NewTurn":
    //                m_tempAudioSource.clip = m_tempSoundArray[3];
    //                break;
    //        }
    //        m_tempAudioSource.Play();
    //    }
    //}

    ///*
    //*/
}