using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;
using System;

// probably should not make this extend RepositionToUICamera as a class, but just require this as a monobehavior.
public class Unit : RepositionToUICamera,
    /* Interfaces */ IPlaceable, IUnit {

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

    IGamePlayer m_playerOwner = null;

    public Tile AssignedToTile { get; set; }

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

        AttackText.text  = GetAttackValue().ToString();
        SHealthText.text = GetSpiritualHealth().ToString();
        PHealthText.text = GetPhysicalHealth().ToString();
    }

    /// IPlaceable Interface Implementations
    bool IPlaceable.IsDragging() {
        //throw new NotImplementedException();
        return m_isDragging;
    }

    bool IPlaceable.AttemptSelection() {
        //cc2d.enabled = true;// 
        if( GetPlayerOwner().Equals(GameManager.GetInstance<GameManager>().CurrentPlayer())  ) {
            m_isDragging = true;
            return m_isDragging;
        }
        return false;
    }

    bool IPlaceable.AttemptRelease( Tile sourceTile, Tile destinationTile ) {
        m_isDragging = false;
        return true;
    }

    public GameObject GetGameObject() {
        return gameObject;
    }
    
    /// IUnit Implementations.
    bool IUnit.Clear() {
        throw new NotImplementedException();
    }


    bool IUnit.IsPhysicalAttack() {
        //throw new NotImplementedException();
        return true;
    }
    bool IUnit.IsSpiritualAttack() {
        return false;
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

    bool IUnit.HasPerformedAction() {
        throw new NotImplementedException();
    }

    public void TakeDamage(int damage, int type) {
        if (type == 0) {
            // physical
            m_pHealth -= damage;
        }
        else {
            m_sHealth -= damage;
        }
        if (m_sHealth == 0 || m_pHealth == 0) {
            if (AssignedToTile != null) {
                AssignedToTile.SetPlaceable(null);
            }
            Destroy(gameObject);
        }
    }


    void IUnit.ModifyPhysicalHealth(int amount){
        throw new NotImplementedException();
    }

    void IUnit.ModifySpiritualHealth(int amount){
        throw new NotImplementedException();
    }

    private void OnMouseDown() {
        //Debug.Log("[Unit] ClickedAsCard");
        m_isDragging = true;
    }

    private void OnMouseUp() {

        RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).x,
                                                          CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition).y),
                                                          Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));
        if (rh2d) {

            //Debug.Log("[Unit] OnMouseUp: rh2d: " + rh2d.transform.name);

            Tile t = rh2d.transform.GetComponent<Tile>();
            if (!t.IsOccupied()) {
                BoxCollider2D bc2d = gameObject.GetComponent<BoxCollider2D>();
                bc2d.enabled = false;

                if(m_playerOwner != null) {
                    List<IUnit> liu = m_playerOwner.GetHand();
                    liu.Remove((IUnit)this);
                }

                t.SetPlaceable(this);
            }
        }

        m_isDragging = false;
    }


    void IUnit.GenerateCardBehavior() {
        BoxCollider2D bc2d = gameObject.GetComponent<BoxCollider2D>();
        bc2d.size = Vector2.one;
        bc2d.enabled = true;
    }

}