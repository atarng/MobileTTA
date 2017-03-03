using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;
using System;

public class Unit : RepositionToUICamera, IPlaceable, IUnit {

    bool m_isDragging = false;
    int m_maxMovement = 2;
    int m_attackRange = 1;

    public CircleCollider2D cc2d;

    // UI Components
    public Text AttackText;
    public Text PHealthText;
    public Text SHealthText;

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

        AttackText.text = ((IUnit)this).GetAttackValue().ToString();
        SHealthText.text = ((IUnit)this).GetSpiritualHealth().ToString();
        PHealthText.text = ((IUnit)this).GetPhysicalHealth().ToString();
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

    void IPlaceable.SetDragging() {

        //cc2d.enabled = true;// 

        m_isDragging = true;
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
    int IUnit.GetAttackValue() {
        //throw new NotImplementedException();
        return 4;
    }
    int IUnit.GetAttackRange() {
        return m_attackRange;
    }
    int IUnit.GetPlayerOwner() {
        throw new NotImplementedException();
    }

    int IUnit.GetMaxMovement() {
        return m_maxMovement;
    }

    int IUnit.GetPhysicalHealth() {
        //throw new NotImplementedException();
        return 4;
    }

    int IUnit.GetSpiritualHealth() {
        //throw new NotImplementedException();
        return 4;
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
