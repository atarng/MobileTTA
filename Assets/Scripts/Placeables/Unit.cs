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
    bool m_isDragging = false;
    int m_maxMovement = 2;
    int m_attackRange = 1;

//public CircleCollider2D cc2d;

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
    IGamePlayer m_playerOwner = null;

    public Tile AssignedToTile { get; set; }

    public void ReadDefinition( UnitManager.UnitDefinition ud ) {
        m_pHealthMax = m_pHealth = ud.PhysicalHealth;
        m_sHealthMax = m_sHealth = ud.SpiritualHealth;
        m_Attack      = ud.AttackValue;
        m_attackType  = ud.AttackType;
        m_attackRange = ud.AttackRange;
        m_maxMovement = ud.Movement;
    }

    // Use this for initialization
    protected override void OnAwake() {
//        cc2d.enabled = false;
    }

    // Update is called once per frame
    protected override void OnUpdate() {
        if ( ((IPlaceable)this).IsDragging() ) {
            Vector3 mouse_to_world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouse_to_world.z = 0;
            // attach to mouse
            transform.position = mouse_to_world;
        }

        if (AssignedToTile || GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(m_playerOwner)) {
            if (GetAttackValue() > 0) {
                AttackText.text = GetAttackValue().ToString();
                AttackText.color = IsSpiritualAttack() ? (IsPhysicalAttack() ? Color.magenta : Color.blue) : Color.red;
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
            m_hasPerformedAction = true;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, m_isDying - 1);
            m_isDying += 0.05f;
            if (transform.localScale.magnitude <= 0.1f) {
                Destroy(gameObject);
            }
        }
    }

    /// IPlaceable Interface Implementations
    bool IPlaceable.IsDragging() {
        //throw new NotImplementedException();
        return m_isDragging;
    }

    bool IPlaceable.AttemptSelection() {
        //cc2d.enabled = true;// 
        if( GetPlayerOwner().Equals(GameManager.GetInstance<GameManager>().CurrentPlayer()) &&
            GetPlayerOwner().GetEnoughActionPoints(1) &&
            !HasPerformedAction() ) {
            m_isDragging = true;
            return m_isDragging;
        }
        return false;
    }

    bool IPlaceable.AttemptRelease( Tile sourceTile, Tile destinationTile, bool resolved) {
        if (m_isDragging) {
            m_hasPerformedAction = resolved;
            if (resolved) {
                GetPlayerOwner().ExpendUnitActionPoint();
            }
        }
        m_isDragging = false;
        return true;
    }

    public GameObject GetGameObject() {
        return gameObject;
    }
    
    /// IUnit Implementations.
    bool IUnit.Clear() {
        //throw new NotImplementedException();
        m_isDragging = false;
        m_hasPerformedAction = false;
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
        //return GameManager.GetInstance<GameManager>().GetPlayer(m_playerId);
        return m_playerOwner;
    }
    void IUnit.AssignPlayerOwner(IGamePlayer player) {
        m_playerOwner = player;
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

    public bool HasPerformedAction() {
        //throw new NotImplementedException();
        return m_hasPerformedAction;
    }

    public void TakeDamage(int damage, int type) {
        if (type == 0) {
            // physical
            m_pHealth -= damage;
        }
        else {
            m_sHealth -= damage;
        }

        if (m_pHealthMax == 20 && m_sHealthMax == 20) {
            m_pHealth = m_sHealth = Mathf.Min(m_sHealth, m_pHealth);
        }

        if (m_sHealth <= 0 || m_pHealth <= 0) {
            if (AssignedToTile != null) {
                AssignedToTile.SetPlaceable(null);
            }
            m_playerOwner.GetCurrentSummonedUnits().Remove(this);
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
        /*
        foreach (IUnit iu in GetPlayerOwner().GetCurrentSummonedUnits()) {
            Unit u = iu.GetGameObject().GetComponent<Unit>();
            List<Tile> lt = GameManager.GetInstance<GameManager>().GetGrid().GetCircumference(u.AssignedToTile, 1);
            if (lt.Contains(t)) {
                return true;
            }
        }
        */
        return false;
    }

    // Only for dragging from Hand
    private void OnMouseDown() {
        //Debug.Log("[Unit] ClickedAsCard");
        if(GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(m_playerOwner) &&
            m_playerOwner.GetEnoughActionPoints(m_playerOwner.GetCurrentSummonedUnits().Count)){
            m_isDragging = true;
            GameManager.GetInstance<GameManager>().GetGrid().DisplaySummonableTiles(m_playerOwner);
        }
    }

    // Only for dragging from Hand
    private void OnMouseUp() {
        if (!m_isDragging) return;
        bool unit_placed = false;
        RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).x,
                                                          CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).y),
                                                          Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));
        if (rh2d) {
            Tile t = rh2d.transform.GetComponent<Tile>();
            if (IsAdjacentToAlliedUnit(t) && !t.IsOccupied()) {
                BoxCollider2D bc2d = gameObject.GetComponent<BoxCollider2D>();
                bc2d.enabled = false;

                if (m_playerOwner != null) {
                    //List<IUnit> liu = m_playerOwner.GetHand();
                    //liu.Remove((IUnit)this);
                    m_playerOwner.PlaceUnitOnField(this);
                }
                unit_placed = true;
                t.SetPlaceable(this);
            }
        }

        if (!unit_placed && m_playerOwner != null) {
            m_playerOwner.RepositionCardsInHand();
        }

        GameManager.GetInstance<GameManager>().GetGrid().ClearPathableTiles();

        m_hasPerformedAction = true;
        m_isDragging = false;
    }


    void IUnit.GenerateCardBehavior() {
        BoxCollider2D bc2d = gameObject.GetComponent<BoxCollider2D>();
        bc2d.size = Vector2.one;
        bc2d.enabled = true;
    }

}