using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;
using System;

// probably should not make this extend RepositionToUICamera as a class, but just require this as a monobehavior.
public class Unit : RepositionToUICamera,
    IPlaceable, IUnit {

    bool m_isDragging = false;
    int m_maxMovement = 2;
    int m_attackRange = 1;

    public CircleCollider2D cc2d;

    // UI Components
    public Text AttackText;
    public Text PHealthText;
    public Text SHealthText;

    int m_pHealth = 4;
    int m_pHealthMax = 4;

    int m_sHealth = 4;
    int m_sHealthMax = 4;

    int m_Attack = 2;

    int m_playerId;

    // Use this for initialization
    protected override void OnAwake() {
        cc2d.enabled = false;
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
/*
    private void OnMouseUp() {
        // only should release if we have a valid tile to release to.
        Debug.Log("[Unit] OnMouseUp: " + name);
        if ( ((IPlaceable)this).IsDragging() ) {
            ((IPlaceable)this).AttemptRelease();
        }
    }
//*/
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

        bool ret = true;
        /*
        int colDiff = Math.Abs(destinationTile.xPos - sourceTile.xPos);
        int rowDiff = Math.Abs(destinationTile.yPos - sourceTile.yPos);

        IPlaceable ItemOnDestTile = destinationTile.GetPlaceable();
        if (ItemOnDestTile != null) {
            // check to see if viable neighbor tiles can hold current unit.

            // attempting to attack or interact. TODO: set attack behavior
            // find a viable destination tile in attack range.

            return false;
        }
        else {
            // TODO: implement actual path finding.

            if (colDiff + rowDiff > ((IUnit)this).GetMaxMovement()) {
                ret = false;
                Debug.LogWarning("[Unit] Attempting to place unit beyond its maximum range.");
            }
        }
        */
        return ret;
    }

    GameObject IPlaceable.GetGameObject() {
        return gameObject;
    }


    /// IUnit Implementations.
    bool IUnit.Clear() {
        throw new NotImplementedException();
    }


    bool IUnit.IsPhysicalAttack() {
        throw new NotImplementedException();
    }
    bool IUnit.IsSpiritualAttack() {
        throw new NotImplementedException();
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
    }
    void IUnit.AssignPlayerOwner(int playerID) {
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

    bool IUnit.HasPerformedAction() {
        throw new NotImplementedException();
    }
    void IUnit.ModifyPhysicalHealth(int amount){
        throw new NotImplementedException();
    }

    void IUnit.ModifySpiritualHealth(int amount){
        throw new NotImplementedException();
    }

}