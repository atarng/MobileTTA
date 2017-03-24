using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;
using System;

// probably should not make this extend RepositionToUICamera as a class, but just require this as a monobehavior.
/* Interfaces IPlaceable, */
public class Unit : MonoBehaviour, IUnit {

    bool m_hasPerformedAction = false;
    public bool HasPerformedAction {
        get {
            return m_hasPerformedAction;
        }
        private set {
            m_hasPerformedAction = value;
            if (m_actionPerformedImage != null && !isNexus() && (AssignedToTile != null)
            ) {
                m_actionPerformedImage.SetActive(m_hasPerformedAction);
            }
        }
    }

    bool m_isDragging = false;
    int m_maxMovement = 2;
    int m_attackRange = 1;

    [SerializeField]
    Transform m_artPlacement;
    [SerializeField]
    GameObject m_actionPerformedImage;

    //[SerializeField]
    //Image m_tempArtRef;
    //[SerializeField]
    //Sprite[] m_tempSpriteArray;

    // UI Components
    public Text AttackText;
    public Text PHealthText;
    public Text SHealthText;

    int m_pHealth = 4;
    int m_pHealthMax = 4;

    int m_sHealth = 4;
    int m_sHealthMax = 4;

    int m_Attack = 2;
    bool m_attackType = false;
    float m_isDying = 0;

    //IGamePlayer m_playerOwner = null;
    int m_playerId = -1;

    static Unit s_selectedUnit = null;
    bool IsSelectedUnit() {
        return this == s_selectedUnit;
    }

    Tile m_assignedToTile = null;
    public Tile AssignedToTile {
        get {
            return m_assignedToTile;
        } set {
            if (m_assignedToTile != null) {
                m_assignedToTile.SetPlaceable(null);
            }
            m_assignedToTile = value;
            if(m_assignedToTile != null) {
                m_assignedToTile.SetPlaceable(this);
            }
            //
        }
    } //private set; }

    public Tile PendingPlacementTile {
        get {
            return m_pendingPlacementTile;
        }
        set {
            TileStateEnum paA_tse = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(m_pendingAttackPlacementTile);
            if (m_pendingAttackPlacementTile != null) {
                m_pendingAttackPlacementTile.sr.color = (paA_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
            }

            TileStateEnum pa_tse = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(m_pendingPlacementTile);
            if (m_pendingPlacementTile != null) {
                m_pendingPlacementTile.sr.color = (pa_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
            }

            m_pendingPlacementTile = value;
            if(m_pendingPlacementTile != null) {
                m_pendingPlacementTile.sr.color = Color.cyan;
                m_pendingPlacementTile.SetPlaceable(this, false);
            }
        }
    }
    private Tile m_pendingPlacementTile = null;
    private Tile m_pendingAttackPlacementTile = null;
    private List<Tile> m_pendingAttackList = null;

    public void ReadDefinition( UnitManager.UnitDefinition ud ) {
        m_pHealthMax = m_pHealth = ud.PhysicalHealth;
        m_sHealthMax = m_sHealth = ud.SpiritualHealth;
        m_Attack      = ud.AttackValue;
        m_attackType  = ud.AttackType;
        m_attackRange = ud.AttackRange;
        m_maxMovement = ud.Movement;

        GameObject artInstance = GameObject.Instantiate( SingletonMB.GetInstance<UnitManager>().GetArtFromKey( ud.ArtKey ) );
        artInstance.transform.SetParent( m_artPlacement );
        artInstance.transform.localPosition = Vector3.zero;
        artInstance.transform.localRotation = Quaternion.identity;

        //if(m_attackRange > 0) {
        //    m_tempArtRef.sprite = m_tempSpriteArray[m_attackRange - 1];
        //}

    }

    bool m_mouseDownSelected = false;
    // Update is called once per frame
    //protected override void OnUpdate() {
    private void Update() {
        Tile currentlyOverTile = null;
        RaycastHit2D rayCastToGridTiles;

        /*** THIS IS FOR CLICK SELECT INPUT ***/
        if (IsSelectedUnit()) {
            if (Input.GetMouseButtonDown(0)) {
                m_lastMousePosition = Input.mousePosition;
                m_mouseDownSelected = true;
            }

            if (m_mouseDownSelected && Vector3.Distance(m_lastMousePosition, Input.mousePosition) > 2) {
                m_isDragging = true;
                s_selectedUnit = null;
                m_mouseDownSelected = false;
                return;
            }

            if (m_mouseDownSelected && Input.GetMouseButtonUp(0)) {
                m_mouseDownSelected = false;

                // TODO: CLEAN THIS SHIT UP
                rayCastToGridTiles = Physics2D.Raycast(new Vector2(CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).x,
                              CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).y),
                              Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));

                if (rayCastToGridTiles) {
                    currentlyOverTile = rayCastToGridTiles.transform.GetComponent<Tile>();
                }
                if (currentlyOverTile == null) return;

                bool resolved = false;
                if (AssignedToTile) {
                    TileStateEnum c_tse = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(currentlyOverTile);

                    switch (c_tse) {
                        case TileStateEnum.CanAttack:
                            //hovering over an enemy
                            if (currentlyOverTile.GetPlaceable() is ICombatPlaceable) {
                                ICombatPlaceable icp = currentlyOverTile.GetPlaceable() as ICombatPlaceable;
                                m_pendingAttackList = GameManager.GetInstance<GameManager>().GetGrid().GetAccessibleAttackPositions(AssignedToTile, currentlyOverTile);

                                Tile previousPendingPlacement = PendingPlacementTile;
                                if (!m_pendingAttackList.Contains(m_pendingAttackPlacementTile)) {
                                    m_pendingAttackPlacementTile = PendingPlacementTile = m_pendingAttackList[0];
                                }

                                ///*
                                if (icp != null) {
                                    //bool matchingTile = (previousPendingPlacement.Equals(PendingPlacementTile));
                                    // in case it's on a different tile/need to restore position.
                                    PendingPlacementTile = m_pendingAttackPlacementTile;
                                    if (previousPendingPlacement.Equals(PendingPlacementTile)) { //(previousPendingPlacement.name == m_pendingPlacementTile.name) {

                                        Debug.Log("previousPendingPlacement: " + previousPendingPlacement.name +
                                                   "PendingPlacement: " + m_pendingPlacementTile.name);

                                        resolved = true;
                                        AssignedToTile = PendingPlacementTile;
                                        SingletonMB.GetInstance<GameManager>().HandleCombat(this, icp);
                                    }
                                }
                                //*/
                            }
                            break;
                        case TileStateEnum.CanMove:
                            if (currentlyOverTile.GetPlaceable() == null) {
                                if (PendingPlacementTile == currentlyOverTile) {
                                    AssignedToTile = currentlyOverTile;
                                    resolved = true;
                                }
                                else {
                                    if (m_pendingAttackList != null && m_pendingAttackList.Contains(currentlyOverTile)){
                                        m_pendingAttackPlacementTile = currentlyOverTile;
                                    }
                                    // Unset original tile, place on new tile.
                                    PendingPlacementTile = currentlyOverTile;
                                }
                            }
                            else {
                                SceneControl.GetCurrentSceneControl().DisplayError("Placeable on Tile Impossible.");
                                AssignedToTile.SetPlaceable(this);
                            }
                            
                            break;
                        case TileStateEnum.CanStay:
                        default:
                            if (m_pendingAttackList != null && m_pendingAttackList.Contains(currentlyOverTile)) {
                                m_pendingAttackPlacementTile = currentlyOverTile;
                                PendingPlacementTile = currentlyOverTile;
                            }
                            else {
                                AssignedToTile.SetPlaceable(this);
                                // Unselect
                                s_selectedUnit = null;
                            }
                            break;
                    }

                    AttemptRelease(resolved);
                    if (resolved || !IsSelectedUnit()) {
                        GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
                    }
                }
            }
        }
        /*** DRAGGING LOGIC ***/
        else if (IsDragging()) {
            Vector3 mouse_to_world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouse_to_world.z = 0;
            // attach to mouse
            transform.position = mouse_to_world;

            //
            rayCastToGridTiles = Physics2D.Raycast(new Vector2(CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).x,
                                          CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).y),
                                          Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));
            currentlyOverTile = null;
            if (rayCastToGridTiles) {
                currentlyOverTile = rayCastToGridTiles.transform.GetComponent<Tile>();
            }
            if (currentlyOverTile == null) return;

            if (AssignedToTile) {

                TileStateEnum c_tse = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(currentlyOverTile);
                TileStateEnum pa_tse = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(m_pendingPlacementTile);
                TileStateEnum paA_tse = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(m_pendingAttackPlacementTile);
                switch (c_tse) {
                    case TileStateEnum.CanAttack:

                        if (currentlyOverTile.GetPlaceable() is ICombatPlaceable) {
                            ICombatPlaceable icp = currentlyOverTile.GetPlaceable() as ICombatPlaceable;
                            m_pendingAttackList = GameManager.GetInstance<GameManager>().GetGrid().GetAccessibleAttackPositions(AssignedToTile, currentlyOverTile);

                            // PENDING ATTACK TILE
                            if (m_pendingAttackPlacementTile != null && m_pendingPlacementTile != m_pendingAttackPlacementTile) {
                                
                                // SNAP BEHAVIOR
                                //PendingPlacementTile = m_pendingPlacementTile;

                                // FREE DRAG
                                if (m_pendingPlacementTile != null) {
                                    m_pendingPlacementTile.sr.color = (pa_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
                                }
                                m_pendingPlacementTile = m_pendingAttackPlacementTile;
                                if (m_pendingPlacementTile != null) {
                                    m_pendingPlacementTile.sr.color = Color.cyan;
                                }
                            }
                            // has not been set yet
                            else if (m_pendingPlacementTile == null || m_pendingAttackPlacementTile == null ||
                                !m_pendingAttackList.Contains(m_pendingAttackPlacementTile) ) {
                                // Free Drag
                                if (m_pendingPlacementTile != null) {
                                    m_pendingPlacementTile.sr.color = (pa_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
                                }

                                m_pendingAttackPlacementTile = m_pendingPlacementTile = m_pendingAttackList[0];
                                if (m_pendingPlacementTile != null) {
                                    m_pendingPlacementTile.sr.color = Color.cyan;
                                }
                            }
                            /*
                            if (m_pendingPlacementTile != null) {
                                Debug.Log("m_pendingPlacementTile: " + m_pendingPlacementTile.name);
                            }
                            */
                        }
                        /*** DEFAULT BEHAVOIR ***/
                        else if (m_pendingAttackPlacementTile != null) {
                            m_pendingAttackPlacementTile.sr.color = (paA_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
                        }
                        break;
                    case TileStateEnum.CanMove:
                    case TileStateEnum.CanStay:
                        if (m_pendingAttackList != null) {
                            // there is a target.
                            int indexOfTile = m_pendingAttackList.FindIndex(currentlyOverTile.MatchesTile);
                            if (indexOfTile >= 0) {
                                // is in attack list.
                                m_pendingAttackPlacementTile = PendingPlacementTile = m_pendingAttackList[indexOfTile];
                            }
                            else {
                                if (m_pendingPlacementTile != null) {
                                    m_pendingPlacementTile.sr.color = (pa_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
                                }
                                if (m_pendingAttackPlacementTile != null) {
                                    m_pendingAttackPlacementTile.sr.color = (paA_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
                                }

                                //m_pendingAttackPlacementTile = currentlyOverTile;
                                //m_pendingAttackPlacementTile.sr.color = Color.cyan;
                                m_pendingPlacementTile = currentlyOverTile;
                                m_pendingPlacementTile.sr.color = Color.cyan;
                            }
                        }
                        else {
                            // Snapping Behavior
                            // PendingPlacementTile = currentlyOverTile;

                            // Free Drag
                            if (m_pendingPlacementTile != null) {
                                m_pendingPlacementTile.sr.color = (pa_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
                            }
                            m_pendingPlacementTile = currentlyOverTile;
                            if (m_pendingPlacementTile != null) {
                                m_pendingPlacementTile.sr.color = Color.cyan;
                            }
                        }
                        break;
                    default: {
                        //*
                        if (m_pendingPlacementTile != null) {
                            m_pendingPlacementTile.sr.color = (pa_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
                        }
                        if (m_pendingAttackPlacementTile != null) {
                            m_pendingAttackPlacementTile.sr.color = (paA_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
                        }
                        m_pendingPlacementTile = null;
                        //*/
                        break;
                    }
                }
            }
        }

        if (isNexus()) {
            PHealthText.enabled = true;
            Vector3 newPos = PHealthText.transform.localPosition;
            newPos.x = 0;
            PHealthText.transform.localPosition = newPos;
            PHealthText.text = GetPhysicalHealth().ToString();

            AttackText.enabled = false;
            SHealthText.enabled = false;

            m_actionPerformedImage.SetActive(!GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(GetPlayerOwner()));
        }
        // Else if Tile is on Board, or it is current players turn.
        else if (AssignedToTile || GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(GetPlayerOwner())) {
            if (GetAttackValue() > 0) {
                AttackText.text = GetAttackValue().ToString();
                AttackText.color = IsSpiritualAttack() ? (IsPhysicalAttack() ? Color.magenta : new Color(0,196, 255)) : Color.red;
                AttackText.enabled = true;
            }
            else {
                AttackText.enabled = false;
            }

            SHealthText.text = GetSpiritualHealth().ToString();
            PHealthText.text = GetPhysicalHealth().ToString();

            SHealthText.enabled = true;
            PHealthText.enabled = true;
        }
        else {
            AttackText.enabled = false;
            SHealthText.enabled = false;
            PHealthText.enabled = false;
        }

        if (m_isDying > 0) {
            HasPerformedAction = true;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, m_isDying - 1);
            m_isDying += 0.05f;
            if (transform.localScale.magnitude <= 0.1f) {
                Destroy(gameObject);
            }
        }
    }

    /// IPlaceable Interface Implementations
    public bool IsDragging() {
        //throw new NotImplementedException();
        return m_isDragging;
    }

    public bool AttemptSelection() {
        //cc2d.enabled = true;// 
        if (GetPlayerOwner().Equals(GameManager.GetInstance<GameManager>().CurrentPlayer())) {
            if ( GetPlayerOwner().GetEnoughActionPoints(1) && !HasPerformedAction && m_maxMovement > 0){
                m_isDragging = true;
                return m_isDragging;
            }
            else if (!(GetPlayerOwner().GetEnoughActionPoints(1))) {
                SceneControl.GetCurrentSceneControl().DisplayWarning("Not enough action points.");
            }
            else if (HasPerformedAction) {
                SceneControl.GetCurrentSceneControl().DisplayWarning("That unit cannot take an action this turn.");
            }
            else {
                Debug.LogWarning("Trying to select invalid unit.");
            }
        }
        return false;
    }

    public bool AttemptRelease(bool resolved ) {// Tile sourceTile, Tile destinationTile, ) {

        if (m_isDragging || IsSelectedUnit()) {
            HasPerformedAction = resolved;
            if (resolved) {
                GetPlayerOwner().ExpendUnitActionPoint();
                s_selectedUnit = null;

                m_pendingAttackList = null;
                m_pendingAttackPlacementTile = null;
                PendingPlacementTile = null;
            }
        }

        // is this one neccessary?
        if (!IsSelectedUnit()) {
            m_pendingAttackList = null;
            PendingPlacementTile = null;
            m_pendingAttackPlacementTile = null;
        }
        m_isDragging = false;
        m_mouseDownSelected = false;

        return true;
    }

    public GameObject GetGameObject() {
        return gameObject;
    }
    
    /// IUnit Implementations.
    bool IUnit.ClearStates() {
        if (IsSelectedUnit()) {
            AssignedToTile = AssignedToTile;
            AttemptRelease(false);
            GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
        }

        m_isDragging = false;
        HasPerformedAction = false;
        return true;
    }


    public bool IsPhysicalAttack() {
        //throw new NotImplementedException();
        return !m_attackType;
    }
    public bool IsSpiritualAttack() {
        return m_attackType;
    }
    public int GetAttackValue() {
        //throw new NotImplementedException();
        return m_Attack;
    }
    int IUnit.GetAttackRange() {
        return m_attackRange;
    }

    public IGamePlayer GetPlayerOwner() {
        return GameManager.GetInstance<GameManager>().GetPlayer(m_playerId);
        //return m_playerOwner;
    }
    void IUnit.AssignPlayerOwner(int playerID) {
        //m_playerOwner = player;
        m_playerId = playerID;
    }

    int IUnit.GetMaxMovement() {
        return m_maxMovement;
    }

    public int GetPhysicalHealth() {
        return m_pHealth;
    }

    public int GetSpiritualHealth() {
        //throw new NotImplementedException();
        return m_sHealth;
    }

    public void TakeDamage(int damage, int type) {
        if (type == 0) {
            // physical
            m_pHealth = Mathf.Max(0, m_pHealth - damage);
        }
        else {
            m_sHealth = Mathf.Max(0, m_sHealth - damage);
        }

        if (isNexus()) {
            m_pHealth = m_sHealth = Mathf.Min(m_sHealth, m_pHealth);
        }

        if (m_sHealth <= 0 || m_pHealth <= 0) {
            if (AssignedToTile != null) {
                AssignedToTile.SetPlaceable(null);
            }
            GetPlayerOwner().GetCurrentSummonedUnits().Remove(this);
            //Destroy(gameObject);
            m_isDying = 1;
        }
    }


    void IUnit.ModifyPhysicalHealth(int amount){
        throw new NotImplementedException();
    }

    void IUnit.ModifySpiritualHealth(int amount){
        throw new NotImplementedException();
    }

    private bool IsAdjacentToAlliedUnit(Tile t) {
        List<Tile> lt = GameManager.GetInstance<GameManager>().GetGrid().GetCircumference(t, 1);
        for (int i = 0; i < lt.Count; i++) {
            IPlaceable ip = lt[i].GetPlaceable();
            if (ip != null) {
                Unit u = ip as Unit;
                if (u != null && u.GetPlayerOwner() == GetPlayerOwner()) {
                    return true;
                }
            }
        }
        return false;
    }


    /********************************************************/

    Vector3 m_lastMousePosition = Vector3.zero;
    private void OnMouseDown() {
        /* Maybe need to move this logic somewhere */
        if (GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(GetPlayerOwner()) &&
            s_selectedUnit != this) {
            s_selectedUnit = null;
            GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
        }

        /*** BEHAVIOR ON GRID ***/
        if( AssignedToTile != null && AttemptSelection()) {
            /*** Might need to move this logic to mouse up code***/
            /*
            if (IsSelectedUnit()) {
                AssignedToTile = PendingPlacementTile;
                AttemptRelease(true);
                GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
            }
            else
            */
            {
                m_lastMousePosition = Input.mousePosition;
                GameManager.GetInstance<GameManager>().GetGrid().DeterminePathableTiles(AssignedToTile, this);
            }
        }
        /****************** BELOW: BEHAVIOR AS CARD ************************/
        else if (AssignedToTile == null &&
            GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(GetPlayerOwner()) &&
            GetPlayerOwner().GetEnoughActionPoints(GetPlayerOwner().GetCurrentSummonedUnits().Count)
        ) {
            m_isDragging = true;
            GameManager.GetInstance<GameManager>().GetGrid().DisplaySummonableTiles(GetPlayerOwner());
        }

    }
    private void OnMouseUp() {
        if (!m_isDragging) return;
        bool action_resolved = true;

        RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).x,
                                                  CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).y),
                                                  Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));
        Tile CurrentlyOverTile = null;
        if (rh2d) {
            CurrentlyOverTile = rh2d.transform.GetComponent<Tile>();
        }

        /*** BEHAVIOR ON GRID ***/
        if (AssignedToTile != null) {

            if (Vector3.Distance(m_lastMousePosition, Input.mousePosition) < 2) {
                s_selectedUnit = this;
                AssignedToTile = AssignedToTile;
                m_isDragging = false;
            }
            else {
                //IPlaceable unit_to_release = m_itemOnTile;
                // TODO: Cleanup this code. Always release drag.
                switch (GameManager.GetInstance<GameManager>().GetGrid().TileStateAt(CurrentlyOverTile)) {  //m_parentGrid.TileStateAt(s_currentTileOver)) {
                    case TileStateEnum.CanMove:
                        if (CurrentlyOverTile.GetPlaceable() != null) {
                            SceneControl.GetCurrentSceneControl().DisplayError("Placeable on Tile Impossible.");
                            AssignedToTile.SetPlaceable(this);
                            action_resolved = false;
                        }
                        else {
                            // Unset original tile, place on new tile.
                            AssignedToTile = CurrentlyOverTile;
                        }
                        break;
                    case TileStateEnum.CanAttack:
                        bool valid = false;

                        ICombatPlaceable icp = CurrentlyOverTile.GetPlaceable() as ICombatPlaceable;
                        if(icp != null && m_pendingPlacementTile != null) { // need to check what to do about this.
                            // Find Tile that can be placed at this location.
                            AssignedToTile = m_pendingPlacementTile;

                            GameManager.GetInstance<GameManager>().HandleCombat(this, icp);
                            valid = true;
                        }

                        if (!valid) {
                            // if target tile is empty then place this tile back on original position.
                            AssignedToTile.SetPlaceable(this);
                            action_resolved = false;
                        }
                        break;
                    case TileStateEnum.CanNotAccess:
                    default:
                        AssignedToTile.SetPlaceable(this);
                        action_resolved = false;
                        break;
                }
                //AssignedToTile, CurrentlyOverTile,
                AttemptRelease(action_resolved);

                //
                //if(action_resolved || !IsSelectedUnit()) {
                GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
                //}
            }
        }
        /*** BEHAVIOR WHILE IN HAND ***/
        else {
            bool unit_placed = false;
            if(CurrentlyOverTile != null) {
                // Tile is not null
                if (IsAdjacentToAlliedUnit( CurrentlyOverTile ) && !CurrentlyOverTile.IsOccupied()) {
                    AssignedToTile = CurrentlyOverTile; //CurrentlyOverTile.SetPlaceable(this);
                    HasPerformedAction = true;

                    // keeping the collider to make it so that it's unit based tile placement instead of tile.
                    // BoxCollider2D bc2d = gameObject.GetComponent<BoxCollider2D>();
                    // bc2d.enabled = false;

                    if (GetPlayerOwner() != null) {
                        GetPlayerOwner().PlaceUnitOnField(this);
                    }
                    unit_placed = true;
                }
            }

            if (!unit_placed && GetPlayerOwner() != null) {
                GetPlayerOwner().RepositionCardsInHand();
            }
            GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
            m_isDragging = false;
        }
    }

    /********************************************************/

    void IUnit.GenerateCardBehavior() {
        BoxCollider2D bc2d = gameObject.GetComponent<BoxCollider2D>();
        bc2d.size = Vector2.one;
        bc2d.enabled = true;
    }
    private bool isNexus() {
        return m_pHealthMax == 200 && m_sHealthMax == 200;
    }
}