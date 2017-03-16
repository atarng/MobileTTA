using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AtRng.MobileTTA;

public class Tile : MonoBehaviour {

    IPlaceable m_itemOnTile;
    Grid       m_parentGrid;

    public SpriteRenderer sr;

    public int xPos { get; set; }
    public int yPos { get; set; }

    //private Tile m_pendingAttackTile = null;
    //static Tile s_currentTileOver;
    //public static Tile CurrentlyOverTile() {
    //    return s_currentTileOver;
    //}

    public bool IsOccupied() {
        return m_itemOnTile != null;
    }

    bool successfullyGrabbed = false;


    public bool MatchesTile(Tile t) {
        return t == this;
    }
    /*
    private void OnMouseEnter(){
        s_currentTileOver = this;
    }
    private void OnMouseExit() {
        if(s_currentTileOver == this) {
            s_currentTileOver = null;
        }
    }
    private void OnMouseDown() {
        if (!successfullyGrabbed) {
            if (m_itemOnTile != null) {
                Unit u = m_itemOnTile.GetGameObject().GetComponent<Unit>();
                IUnit iu = u;
                if (m_itemOnTile.AttemptSelection()) {
                    if (u != null) {
                        m_parentGrid.DeterminePathableTiles(this, u);
                    }
                    successfullyGrabbed = true;
                }
            }
        }
    }
    private void OnMouseUp(){
        if (!successfullyGrabbed) return;
        bool action_resolved = true;
        if (m_itemOnTile != null) {
            IPlaceable unit_to_release = m_itemOnTile;
            // TODO: Cleanup this code. Always release drag.
            switch (m_parentGrid.TileStateAt(s_currentTileOver)) {
                case TileStateEnum.CanMove:
                    if (s_currentTileOver.GetPlaceable() != null) {
                        SetPlaceable(m_itemOnTile);
                        action_resolved = false;
                    }
                    else {
                        s_currentTileOver.SetPlaceable(m_itemOnTile);
                        m_itemOnTile = null;
                    }
                    break;
                case TileStateEnum.CanAttack:
                    //TODO
                    // Find Tile that can be placed at this location.
                    if (s_currentTileOver.m_itemOnTile != null) {
                        List<Tile> canAttackFrom = m_parentGrid.GetAccessibleAttackPositions(this, s_currentTileOver);
                        if (canAttackFrom.Count == 1) {
                            canAttackFrom[0].SetPlaceable(m_itemOnTile);
                            GameManager.GetInstance<GameManager>().HandleCombat(m_itemOnTile, s_currentTileOver.m_itemOnTile);
                            if (canAttackFrom[0] != this) {
                                // we don't want to do this if tile is still on this location.
                                m_itemOnTile = null;
                            }
                        }
                        else {
                            if (m_pendingAttackTile != null) {
                                // confirm attack position at pending tile
                                m_pendingAttackTile.SetPlaceable( m_itemOnTile );
                                GameManager.GetInstance<GameManager>().HandleCombat(m_itemOnTile, s_currentTileOver.m_itemOnTile);
                                if (m_pendingAttackTile != this) {
                                    m_itemOnTile = null;
                                }
                            }
                            else {
                                m_pendingAttackTile = canAttackFrom[0];
                                //m_pendingAttackTile.collider2D.
                                action_resolved = false;
                            }
                        }
                        break;
                    }
                    else {
                        // if target tile is empty then place this tile back on original position.
                        SetPlaceable(m_itemOnTile);
                        action_resolved = false;
                    }
                    break;
                case TileStateEnum.CanNotAccess:
                default:
                    SetPlaceable(m_itemOnTile);
                    action_resolved = false;
                    break;
            }
            unit_to_release.AttemptRelease(this, s_currentTileOver, action_resolved);
        }
        m_parentGrid.ClearPathableTiles();
        successfullyGrabbed = false;
    }
*/
    public void SetParentGrid(Grid parentGrid) {
        m_parentGrid = parentGrid;
    }

    public void SetPlaceable(IPlaceable toSet){
        m_itemOnTile = toSet;
        if(toSet != null) {
            toSet.GetGameObject().transform.position = transform.position;
            toSet.GetGameObject().transform.SetParent(transform);

            //toSet.AssignedToTile = this; // atarng: change to be based on unit instead of tile.

        }
    }
    public IPlaceable GetPlaceable() {
        return m_itemOnTile;
    }



}