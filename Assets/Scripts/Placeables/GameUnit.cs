using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;
using System;

/***
 * GameUnit Class is the Unit Class which is handled by the player.
 *  
 */
public class GameUnit : BaseUnit, ICombat {
    static GameUnit s_selectedUnit = null;

    #region PRIVATE_MEMBERS
    bool m_hasPerformedAction = false;
    bool m_mouseDownSelected = false;
    float m_isDying = 0;
    private Tile m_pendingPlacementTile = null;
    private Tile m_pendingAttackPlacementTile = null;
    private Tile m_currentTarget = null;
    private List<Tile> m_pendingAttackList = null;

    [SerializeField]
    private GameObject m_actionPerformedImage;
    private ISoundManager m_soundManagerTemp;
    private Vector3 m_lastMousePosition = Vector3.zero;
    #endregion

    #region PUBLIC MEMBERS
    // UI Components
    public Text AttackText;
    public Text PHealthText;
    public Text SHealthText;
    #endregion

    // override from base
    public override bool HasPerformedAction {
        get {
            return m_hasPerformedAction;
        }
        protected set {
            m_hasPerformedAction = value;
            if (m_actionPerformedImage != null && !IsNexus() && (AssignedToTile != null)) {
                m_actionPerformedImage.SetActive(m_hasPerformedAction);
            }
        }
    }

    private bool IsSelectedUnit() {
        return this == s_selectedUnit;
    }

    private Tile PendingPlacementTile {
        get {
            return m_pendingPlacementTile;
        }
        set {
            // just sets the color
            TileStateEnum pendingAttackState = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(m_pendingAttackPlacementTile);
            if (m_pendingAttackPlacementTile != null) {
                m_pendingAttackPlacementTile.Sprite.color = (pendingAttackState == TileStateEnum.CanMove) ? TileColors.BLUE : TileColors.WHITE;
            }

            // set color and tile value
            TileStateEnum pendingPlacementState = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(m_pendingPlacementTile);
            if (m_pendingPlacementTile != null) {
                m_pendingPlacementTile.Sprite.color = (pendingPlacementState == TileStateEnum.CanMove) ? TileColors.BLUE : TileColors.WHITE;
            }
            m_pendingPlacementTile = value;
            if (m_pendingPlacementTile != null) {
                m_pendingPlacementTile.Sprite.color = TileColors.CYAN;

                //
                if (!IsDragging()) {
                    m_pendingPlacementTile.SetPlaceable(this, false);
                }

            }
        }
    }
    private Tile CurrentTarget {
        get {
            return m_currentTarget;
        }
        set {
            // RESET COLOR
            if (m_currentTarget != null) {
                TileStateEnum tse = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(m_currentTarget);
                switch (tse) {
                    case TileStateEnum.CanAttack:
                        m_currentTarget.Sprite.color = TileColors.RED;//Color.red;
                        break;
                    case TileStateEnum.CanMove:
                        m_currentTarget.Sprite.color = TileColors.BLUE;
                        break;
                    default:
                        m_currentTarget.Sprite.color = TileColors.WHITE; // WHITE;
                        break;
                }
            }

            // UPDATE TARGET
            m_currentTarget = value;
            if (m_currentTarget != null) {
                m_currentTarget.Sprite.color = TileColors.PINK;
            }
        }
    }

    // Called Every Frame... (ReadDefinition?)
    private void Update_UnitSelectionBehavior() {
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
                            if (currentlyOverTile.GetPlaceable() is ICombat) {
                                ICombat icp = currentlyOverTile.GetPlaceable() as ICombat;
                                m_pendingAttackList = GameManager.GetInstance<GameManager>().GetGrid().GetAccessibleAttackPositions(AssignedToTile, currentlyOverTile);

                                Tile previousPendingPlacement = PendingPlacementTile;
                                if (!m_pendingAttackList.Contains(m_pendingAttackPlacementTile)) {
                                    m_pendingAttackPlacementTile = PendingPlacementTile = m_pendingAttackList[0];
                                }

                                ///*
                                if (icp != null) {
                                    // in case it's on a different tile/need to restore position.
                                    PendingPlacementTile = m_pendingAttackPlacementTile;

                                    if (previousPendingPlacement != null && previousPendingPlacement.Equals(PendingPlacementTile)) { //(previousPendingPlacement.name == m_pendingPlacementTile.name) {
                                        resolved = true;
                                        AssignedToTile = PendingPlacementTile;
                                        SingletonMB.GetInstance<GameManager>().HandleCombat(this, icp);
                                    }
                                }

                                CurrentTarget = currentlyOverTile;

                                //*/
                            }
                            else {
                                CurrentTarget = null;
                            }
                            break;
                        case TileStateEnum.CanMove:
                            if (currentlyOverTile.GetPlaceable() == null) {
                                // Match: Commit Action
                                if (PendingPlacementTile == currentlyOverTile) {
                                    AssignedToTile = currentlyOverTile;
                                    resolved = true;
                                }
                                else {
                                    // If In Attack List, update pending attack position
                                    // Update Pending Placement
                                    // If not in attack List Clear Current Target if it is set.
                                    if (m_pendingAttackList != null && m_pendingAttackList.Contains(currentlyOverTile)) {
                                        m_pendingAttackPlacementTile = currentlyOverTile;
                                    }
                                    else {
                                        CurrentTarget = null;
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
                                //m_pendingPlacementTile.SetPlaceable(this, false);
                            }
                            else {
                                // Unset and restore position
                                AssignedToTile.SetPlaceable(this);
                                s_selectedUnit = null;
                                CurrentTarget = null;
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
            if (!Input.GetMouseButton(0)) {
                //Debug.LogWarning("No More Input!");
                AssignedToTile = AssignedToTile;
                AttemptRelease(false);
                GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
                return;
            }

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

                        if (currentlyOverTile.GetPlaceable() is ICombat) {
                            m_pendingAttackList = GameManager.GetInstance<GameManager>().GetGrid().GetAccessibleAttackPositions(AssignedToTile, currentlyOverTile);

                            // Restore Pending Attack tile
                            if (m_pendingAttackPlacementTile != null && m_pendingPlacementTile != m_pendingAttackPlacementTile) {
                                PendingPlacementTile = m_pendingAttackPlacementTile;
                            }
                            // has not been set yet
                            else if (m_pendingPlacementTile == null || m_pendingAttackPlacementTile == null ||
                                !m_pendingAttackList.Contains(m_pendingAttackPlacementTile)) {
                                m_pendingAttackPlacementTile = PendingPlacementTile = m_pendingAttackList[0];
                            }
                            CurrentTarget = currentlyOverTile;
                        }
                        /*** DEFAULT BEHAVOIR ***/
                        else if (PendingPlacementTile != null || m_pendingAttackPlacementTile != null) {
                            PendingPlacementTile = null;
                            CurrentTarget = null;
                        }
                        break;
                    case TileStateEnum.CanMove:
                    case TileStateEnum.CanStay:
                        if (m_pendingAttackList != null) {
                            PendingPlacementTile = currentlyOverTile;

                            int indexOfTile = m_pendingAttackList.FindIndex(currentlyOverTile.MatchesTilePredicate);
                            if (indexOfTile >= 0) {
                                // is in attack list.
                                m_pendingAttackPlacementTile = m_pendingPlacementTile = m_pendingAttackList[indexOfTile];
                            }
                            else {
                                CurrentTarget = null;
                            }
                            m_pendingPlacementTile.Sprite.color = TileColors.CYAN;
                        }
                        else {
                            if (m_pendingPlacementTile != null) {
                                m_pendingPlacementTile.Sprite.color = (pa_tse == TileStateEnum.CanMove) ? TileColors.BLUE : TileColors.WHITE;
                            }
                            m_pendingPlacementTile = currentlyOverTile;
                            if (m_pendingPlacementTile != null) {
                                m_pendingPlacementTile.Sprite.color = TileColors.CYAN;
                            }
                        }
                        break;
                    default:
                        //*
                        if (m_pendingPlacementTile != null) {
                            m_pendingPlacementTile.Sprite.color = (pa_tse == TileStateEnum.CanMove) ? TileColors.BLUE : TileColors.WHITE;
                        }
                        if (m_pendingAttackPlacementTile != null) {
                            m_pendingAttackPlacementTile.Sprite.color = (paA_tse == TileStateEnum.CanMove) ? TileColors.BLUE : TileColors.WHITE;
                        }
                        m_pendingPlacementTile = null;
                        CurrentTarget = null;
                        break;
                }
            }
        }
    }

    // This is called every Frame... try to move stuff out of this location...
    private void Update_Visuals() {
        if (m_artInstance != null) {
            m_artInstance.SetActive(true);
        }

        if (m_playerId != 0) {
            m_uiOverlay.localRotation = Quaternion.Euler(0, 0, 180);
        }
        float euler_rot = !IsCurrentPlayerTurn ? 180 : 0;
        AttackText.transform.localRotation = Quaternion.Euler(0, 0, euler_rot);
        PHealthText.transform.localRotation = Quaternion.Euler(0, 0, euler_rot);
        SHealthText.transform.localRotation = Quaternion.Euler(0, 0, euler_rot);


        if (IsNexus()) {
            PHealthText.enabled = true;
            Vector3 newPos = PHealthText.transform.localPosition;
            newPos.x = 0;
            PHealthText.transform.localPosition = newPos;
            PHealthText.text = GetPhysicalHealth().ToString();

            AttackText.enabled = false;
            SHealthText.enabled = false;

            m_actionPerformedImage.SetActive(!GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(GetPlayerOwner()));
        }
        // Else if Tile is on Board, or it is current players turn, display text.
        else if ( AssignedToTile
            || GetPlayerOwner() == null
            || GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(GetPlayerOwner())) {
            if (GetAttackValue() > 0) {
                AttackText.text = GetAttackValue().ToString();
                AttackText.color = IsSpiritualAttack() ? (IsPhysicalAttack() ? Color.magenta : new Color(0, 196, 255)) : Color.red;
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

            m_artInstance.SetActive(false);
        }

        if (m_isDying > 0) {
            HasPerformedAction = true;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, m_isDying - 1);
            m_isDying += 0.05f;
            if (transform.localScale.magnitude <= 0.001f) {
                Destroy(gameObject);
            }
        }
    }
    // Update is called once per frame
    private void Update() {
        Update_UnitSelectionBehavior();
        Update_Visuals();
    }

    /// IPlaceable Interface Implementations
    public override bool AttemptSelection() {
        //cc2d.enabled = true;// 
        if (GetPlayerOwner() == null || GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(GetPlayerOwner())) {
            if (!HasPerformedAction && GetMaxMovement() > 0 && 
                (IsMilitia() || GameManager.GetInstance<GameManager>().CurrentPlayer().GetEnoughActionPoints(1) )) {
                m_isDragging = true;
                return true;
            }
            else if (!(GameManager.GetInstance<GameManager>().CurrentPlayer().GetEnoughActionPoints(1))) {
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

    public override bool AttemptRelease(bool resolved) {

        if (m_isDragging || IsSelectedUnit()) {
            HasPerformedAction = resolved;
            if (resolved) {
                GetPlayerOwner().ExpendUnitActionPoint();
                s_selectedUnit = null;

                m_pendingAttackList = null;
                m_pendingAttackPlacementTile = null;
                PendingPlacementTile = null;

                
                if(m_soundManagerTemp == null) {
                    m_soundManagerTemp = SingletonMB.GetInstance<GameManager>();
                }
                m_soundManagerTemp.PlaySound("Tile");
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

        //GameManager.GetInstance<GameManager>().BroadcastEvent("ReleaseUnit");

        return true;
    }

    // Called When Ending Turn.
    public override bool ClearStates() {
        bool ret = base.ClearStates();
        if (IsSelectedUnit()) {
            AssignedToTile = AssignedToTile;
            AttemptRelease(false);
            GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
        }
        return ret;
    }

    public override IGamePlayer GetPlayerOwner() {
        return GameManager.GetInstance<GameManager>().GetPlayer(m_playerId);
        //return m_playerOwner;
    }

    bool ICombat.IsAlive() {
        return GetPhysicalHealth() > 0 && GetSpiritualHealth() > 0;
    }
    private void UpdatePlayerHealth(int playerHealth) {
        GetPlayerOwner().UpdatePlayerHealth(playerHealth);
    }
    public void TakeDamage(int pdamage, int sdamage) {
        // physical
        m_pHealth = Mathf.Max(0, m_pHealth - pdamage);
        m_sHealth = Mathf.Max(0, m_sHealth - sdamage);

        if (IsNexus()) {
            UpdatePlayerHealth(m_pHealth = m_sHealth = Mathf.Min(m_sHealth, m_pHealth));
        }

        if (m_sHealth <= 0 || m_pHealth <= 0) {
            if (AssignedToTile != null) {
                AssignedToTile.SetPlaceable(null);
            }
            GetPlayerOwner().GetCurrentSummonedUnits().Remove(this);
            m_isDying = 1;
        }
    }

    // This Function returns whether or not this tile is adjacent to other units... not sure why
    // it's on this class... Should Move this to grid...
    private bool IsAdjacentToAlliedUnit(Tile t) {
        List<Tile> lt = GameManager.GetInstance<GameManager>().GetGrid().GetCircumference(t, 1);
        for (int i = 0; i < lt.Count; i++) {
            IPlaceable ip = lt[i].GetPlaceable();
            if (ip != null) {
                GameUnit u = ip as GameUnit;
                if (u != null && u.GetPlayerOwner() == GetPlayerOwner()) {
                    return true;
                }
            }
        }
        return false;
    }


    /********************************************************/
    private bool IsCurrentPlayerTurn {
        get { return GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(GetPlayerOwner()); }
    }

    protected virtual void OnMouseDown() {
        /* Maybe need to move this logic somewhere */
        if (IsCurrentPlayerTurn &&
            s_selectedUnit != this) {
            s_selectedUnit = null;
            GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
        }

        /*** BEHAVIOR ON GRID ***/
        if( AssignedToTile != null && AttemptSelection()) {
            m_lastMousePosition = Input.mousePosition;
            GameManager.GetInstance<GameManager>().GetGrid().DeterminePathableTiles(AssignedToTile, this);
        }
        /****************** BELOW: BEHAVIOR AS CARD ************************/
        else if (AssignedToTile == null &&
            GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(GetPlayerOwner()) &&
            GetPlayerOwner().GetEnoughActionPoints(GetPlayerOwner().GetCurrentSummonedUnits().Count) // this cost includes the nexus...
        ) {
            m_isDragging = true;
            SingletonMB.GetInstance<GameManager>().GetGrid().DisplaySummonableTiles(GetPlayerOwner());
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

                        ICombat icp = CurrentlyOverTile.GetPlaceable() as ICombat;
                        if(icp != null && m_pendingPlacementTile != null) { // need to check what to do about this.
                            // Find Tile that can be placed at this location.
                            AssignedToTile = m_pendingPlacementTile;

                            GameManager.GetInstance<GameManager>().HandleCombat(this, icp);
                            valid = true;

                            //
                            CurrentTarget = CurrentlyOverTile;
                        }

                        if (!valid) {
                            // if target tile is empty then place this tile back on original position.
                            AssignedToTile.SetPlaceable(this);
                            action_resolved = false;
                            CurrentTarget = null;
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
                GameManager.GetInstance<GameManager>().BroadcastEvent("ReleaseUnit");
            }
        }
        /*** BEHAVIOR WHILE IN HAND ***/
        else {
            bool unit_placed = false;
            if(CurrentlyOverTile != null) {
                // Tile is not null
                if (IsAdjacentToAlliedUnit( CurrentlyOverTile ) && !CurrentlyOverTile.IsOccupied()) {
                    AssignedToTile = CurrentlyOverTile; //CurrentlyOverTile.SetPlaceable(this);

                    if (GetPlayerOwner() != null) {
                        GetPlayerOwner().PlaceUnitOnField(this);
                    }
                    unit_placed = true;

                    /*** not sure if this is good for this ***/
                    // AR
                    AttemptRelease(unit_placed);
                    // HasPerformedAction = true;

                }
            }


            if (!unit_placed && GetPlayerOwner() != null) {
                GetPlayerOwner().RepositionCardsInHand();
            }
            GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
            m_isDragging = false;

            GameManager.GetInstance<GameManager>().BroadcastEvent("ReleaseUnit");
        }
    }

    /********************************************************/

    public bool IsMilitia() {
        return m_definitionID == 10;
    }
}