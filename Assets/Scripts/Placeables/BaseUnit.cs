﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;
using System;

public abstract class BaseUnit : MonoBehaviour, IUnit {
    // Properties
    // PRIVATE
    private int m_maxMovement = 2;
    private int m_attackRange = 1;
    private int m_attackType = 0;
    private int m_pHealthMax = 4;
    private int m_sHealthMax = 4;
    private int m_Attack = 2;
    private GameObject m_artInstance = null;
    private TileTraversalEnum m_canTraverse = TileTraversalEnum.WalkAndClimb;

    // PROTECTED
    protected bool m_isDragging = false;
    protected int  m_pHealth = 4;
    protected int  m_sHealth = 4;
    protected int  m_playerId   = -1;
    protected int  m_definitionID = -1;
    [SerializeField]
    protected Transform m_artPlacement;

    public int GetID() {
        return m_definitionID;
    }
    public virtual bool HasPerformedAction { get; protected set; }

    public TileTraversalEnum CanTraverse {
        get { return m_canTraverse; }
    }

    Tile m_assignedToTile = null;
    public Tile AssignedToTile {
        get {
            return m_assignedToTile;
        }
        set {
            if (m_assignedToTile != null) {
                m_assignedToTile.SetPlaceable(null);
            }
            m_assignedToTile = value;
            if (m_assignedToTile != null) {
                m_assignedToTile.SetPlaceable(this);
            }
        }
    }

    public void AssignPlayerOwner(int playerID) {
        m_playerId = playerID;
    }
    public abstract IGamePlayer GetPlayerOwner();

    public abstract bool AttemptRelease(bool resolved);
    public abstract bool AttemptSelection();

    public bool IsDragging() {
        return m_isDragging;
    }

    public virtual bool ClearStates() {
        m_isDragging = false;
        HasPerformedAction = false;
        return true;
    }

    public int GetAttackValue() {
        return m_Attack;
    }

    //int IUnit.GetAttackRange() {
    public int GetAttackRange() {
        return m_attackRange;
    }

    public GameObject GetGameObject() {
        return gameObject;
    }

    public int GetMaxMovement() {
        return m_maxMovement;
    }

    public int GetPhysicalHealth() {
        return m_pHealth;
    }

    public int GetSpiritualHealth() {
        return m_sHealth;
    }

    public bool IsPhysicalAttack() {
        return m_attackType == 0 || m_attackType == 2;
    }
    public bool IsSpiritualAttack() {
        return m_attackType == 1 || m_attackType == 2;
    }

    private int m_abilityID = -1;

    /*** ***/
    public void ReadDefinitionID(int defID) {
        UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(defID);
        if (ud != null) {
            ReadDefinition(ud);
        }
        else {
            SceneControl.GetCurrentSceneControl().DisplayError("No Definition Available for that id.");
        }
    }
    public void ReadDefinition(UnitManager.UnitDefinition ud) {
        m_definitionID = ud.DefinitionID;
        m_pHealthMax = m_pHealth = ud.PhysicalHealth;
        m_sHealthMax = m_sHealth = ud.SpiritualHealth;
        m_Attack = ud.AttackValue;
        m_attackType = ud.AttackType;
        m_attackRange = ud.AttackRange;
        m_maxMovement = ud.Movement;
        //m_abilityID = ud.AbilityID;

        ArtPrefab ap = SingletonMB.GetInstance<UnitManager>().GetArtFromKey(ud.ArtKey);
        if (ap != null) {
            if (m_artInstance != null) {
                Destroy(m_artInstance);
            }
            m_artInstance = GameObject.Instantiate(ap.gameObject);
            m_artInstance.transform.SetParent(m_artPlacement);

            m_artInstance.transform.localPosition = Vector3.zero;
            m_artInstance.transform.localRotation = Quaternion.identity;
            m_artInstance.transform.localScale = Vector3.one;
        }
        else {
            SceneControl.GetCurrentSceneControl().DisplayError(string.Format("{0} error!", ud.ArtKey));
        }
    }

/**********************************/

    public bool IsNexus() {
        //return m_pHealthMax == 200 && m_sHealthMax == 200;
        return m_definitionID == 0
            || m_definitionID == 8;
    }
}