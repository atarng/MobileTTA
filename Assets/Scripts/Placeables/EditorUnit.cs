#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using AtRng.MobileTTA;
using System;

public class EditorUnit : BaseUnit {
    static EditorUnit s_pendingPlacementUnit = null;
    //private int m_playerID = -1;
    public void UpdateDefinitionID(int defID) {
        m_definitionID = defID;
    }
    //public void UpdatePlayerID(int playerID) {
    //    m_playerID = playerID;
    //}

    public override bool AttemptRelease(bool resolved) {
        //Debug.Log("[EditorUnit/AttemptRelease]");
        if (s_pendingPlacementUnit != null) {
            bool unitPlaced = false;
            Vector3 v3 = CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(v3.x, v3.y),
                              Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Grid"));
            Tile CurrentlyOverTile = null;
            if (rh2d) {
                CurrentlyOverTile = rh2d.transform.GetComponent<Tile>();
            }
            if (CurrentlyOverTile != null) {
                if (!CurrentlyOverTile.IsOccupied()) {
                    s_pendingPlacementUnit.AssignedToTile = CurrentlyOverTile;
                    s_pendingPlacementUnit.AssignPlayerOwner(1);

                    EditorManager em = ((EditorManager)SceneControl.GetCurrentSceneControl());
                    if (em != null) {
                        em.LevelEditorGrid.AddToPlaceablesList(
                            s_pendingPlacementUnit.m_definitionID,
                            CurrentlyOverTile.xPos, CurrentlyOverTile.yPos,
                            PlaceableType.Unit);
                    }

                    s_pendingPlacementUnit = null;
                    unitPlaced = true;
                }
            }

            if(!unitPlaced){
                Destroy(s_pendingPlacementUnit.gameObject);
                s_pendingPlacementUnit = null;

                if (AssignedToTile != null) {
                    EditorManager em = ((EditorManager)SceneControl.GetCurrentSceneControl());
                    if (em != null) {
                        em.LevelEditorGrid.RemoveFromPlaceables(this);
                    }
                }
            }
        }
        return true;
    }

    public override bool AttemptSelection() {
        //Debug.Log("[EditorUnit/AttemptSelection]");
        if (m_playerId < 0) {//GetPlayerOwner() == null) {
            UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(m_definitionID);
            s_pendingPlacementUnit = GameObject.Instantiate(this);
            s_pendingPlacementUnit.ReadDefinition(ud);

            //s_pendingPlacementUnit.AssignPlayerOwner();
            //s_pendingPlacementUnit.AttemptSelection();

            s_pendingPlacementUnit.transform.SetParent(transform);
            s_pendingPlacementUnit.transform.localScale = Vector3.one;
            s_pendingPlacementUnit.transform.localRotation = Quaternion.identity;
        }
        else {
            s_pendingPlacementUnit = this;
        }
        return true;
    }

    private void OnMouseDown() {
        AttemptSelection();
    }
    private void OnMouseUp() {
        AttemptRelease(true);
    }
    public override IGamePlayer GetPlayerOwner() {
        return null;
    }
}
#endif