using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;
using System;

// probably should not make this extend RepositionToUICamera as a class, but just require this as a monobehavior.
/* Interfaces IPlaceable, */
public class Unit : RepositionToUICamera, IUnit {

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
    private Tile m_pendingAttackTile = null;
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

    // Use this for initialization
    protected override void OnAwake() {
//        cc2d.enabled = false;
    }

    // Update is called once per frame
    protected override void OnUpdate() {
        if (IsDragging()) {
            Vector3 mouse_to_world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouse_to_world.z = 0;
            // attach to mouse
            transform.position = mouse_to_world;

            //
            RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).x,
                                          CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).y),
                                          Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));
            Tile CurrentlyOverTile = null;
            if (rh2d) {
                CurrentlyOverTile = rh2d.transform.GetComponent<Tile>();
            }
            if (CurrentlyOverTile == null) return;

            if (AssignedToTile) {
                TileStateEnum c_tse = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(CurrentlyOverTile);
                switch (c_tse) {
                    case TileStateEnum.CanAttack:
                        if (CurrentlyOverTile.GetPlaceable() is ICombatPlaceable) {
                            ICombatPlaceable icp = CurrentlyOverTile.GetPlaceable() as ICombatPlaceable;
                            m_pendingAttackList = GameManager.GetInstance<GameManager>().GetGrid().GetAccessibleAttackPositions(AssignedToTile, CurrentlyOverTile);
                            if (m_pendingAttackTile == null) {
                                m_pendingAttackTile = m_pendingAttackList[0];
                                m_pendingAttackTile.sr.color = Color.cyan;
                            }
                        }
                        break;
                    case TileStateEnum.CanMove:
                    case TileStateEnum.CanStay:
                        if (m_pendingAttackList != null) {
                            int indexOfTile = m_pendingAttackList.FindIndex(CurrentlyOverTile.MatchesTile);
                            if (indexOfTile >= 0 ){
                                //switch (SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(m_pendingAttackTile)) {
                                //    case TileStateEnum.CanMove:

                                //}
                                TileStateEnum pa_tse = SingletonMB.GetInstance<GameManager>().GetGrid().TileStateAt(m_pendingAttackTile);
                                m_pendingAttackTile.sr.color = (pa_tse == TileStateEnum.CanMove) ? Color.blue : Color.white;
                                m_pendingAttackTile = m_pendingAttackList[indexOfTile];
                                m_pendingAttackTile.sr.color = Color.cyan;
                            }
                        }
                        break;
                    default:
                        break;
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

    public bool AttemptRelease( Tile sourceTile, Tile destinationTile, bool resolved) {
        if (m_isDragging) {
            HasPerformedAction = resolved;
            if (resolved) {
                GetPlayerOwner().ExpendUnitActionPoint();
            }
        }
        m_isDragging = false;

        m_pendingAttackList = null;
        m_pendingAttackTile = null;

        return true;
    }

    public GameObject GetGameObject() {
        return gameObject;
    }
    
    /// IUnit Implementations.
    bool IUnit.Clear() {
        //throw new NotImplementedException();
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
    private void OnMouseDown() {
        //if (!successfullyGrabbed) {
        //    if (m_itemOnTile != null) {
        //        Unit u = m_itemOnTile.GetGameObject().GetComponent<Unit>();
        //        IUnit iu = u;
        //        if (m_itemOnTile.AttemptSelection()) {
        if( AssignedToTile != null && AttemptSelection()) {
            GameManager.GetInstance<GameManager>().GetGrid().DeterminePathableTiles(AssignedToTile, this);
            //successfullyGrabbed = true;
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

        if (AssignedToTile != null) {
            //IPlaceable unit_to_release = m_itemOnTile;
            // TODO: Cleanup this code. Always release drag.
            switch (GameManager.GetInstance<GameManager>().GetGrid().TileStateAt( CurrentlyOverTile )) {  //m_parentGrid.TileStateAt(s_currentTileOver)) {
                case TileStateEnum.CanMove:
                    if (CurrentlyOverTile.GetPlaceable() != null) { //s_currentTileOver.GetPlaceable() != null) {
                        Debug.LogWarning("This Shouldn't Happen.");
                        AssignedToTile.SetPlaceable(this);
                        action_resolved = false;
                    }
                    else {
                        // Unset original tile, place on new tile.
                        //CurrentlyOverTile.SetPlaceable(this);
                        //AssignedToTile.SetPlaceable(null); //m_itemOnTile = null;


                        AssignedToTile = CurrentlyOverTile;
                    }
                    break;
                case TileStateEnum.CanAttack:
                    //TODO
                    // Find Tile that can be placed at this location.
                    if (m_pendingAttackTile != null) {
                        AssignedToTile = m_pendingAttackTile;
                        ICombatPlaceable icp = CurrentlyOverTile.GetPlaceable() as ICombatPlaceable;
                        GameManager.GetInstance<GameManager>().HandleCombat(this, icp);
                    }
                    else {
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
            AttemptRelease(AssignedToTile, CurrentlyOverTile, action_resolved);
            GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();
        }
        /*************************************************************************/
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