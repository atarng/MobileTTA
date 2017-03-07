using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AtRng.MobileTTA;

public class Tile : MonoBehaviour {

    IPlaceable   m_itemOnTile;
    Grid m_parentGrid;
    bool successfullyGrabbed = false;

    public SpriteRenderer sr;

    public int xPos { get; set; }
    public int yPos { get; set; }

    static Tile s_currentTileOver;
    public static Tile CurrentlyOverTile() {
        return s_currentTileOver;
    }
    public bool IsOccupied() {
        return m_itemOnTile != null;
    }

    private void OnMouseDown() {
        if (!successfullyGrabbed) {
//      Debug.Log("[Tile] OnMouseDown: " + name);
            if (m_itemOnTile != null) {
                Unit u = m_itemOnTile.GetGameObject().GetComponent<Unit>();
                IUnit iu = u;
                if (m_itemOnTile.AttemptSelection() ) {
                    //iu.GetPlayerOwner().Equals(GameManager.GetInstance<GameManager>().CurrentPlayer()) ) {

                    if (u != null) {
                        m_parentGrid.DeterminePathableTiles(this, u);
                    }
                    successfullyGrabbed = true;
                }
            }
        }
    }

    private void OnMouseEnter(){
        s_currentTileOver = this;
    }
    private void OnMouseExit() {
        if(s_currentTileOver == this) {
            s_currentTileOver = null;
        }
    }

    private void OnMouseUp(){
        if (!successfullyGrabbed) return;

        //Debug.Log("[Tile] OnMouseUp: " + name);
        // ray cast doesn't work with Collider 2d.
        bool turn_finished = true;
        if (m_itemOnTile != null) {
            /*
            //m_itemOnTile.SetDragging();
            if (m_itemOnTile.AttemptRelease( this, s_currentTileOver)) {
                //Debug.Log(string.Format("[{0}/OnMouseUp] Set on tile: {1}", name, s_currentTileOver.name));
                //m_itemOnTile.GetGameObject().name ));
                s_currentTileOver.SetPlaceable(m_itemOnTile);
                m_itemOnTile = null;
            }
            else {
                
            }
            */

            // TODO: Cleanup this code. Always release drag.
            m_itemOnTile.AttemptRelease(this, s_currentTileOver);

            switch (m_parentGrid.TileStateAt(s_currentTileOver)) {
                case TileStateEnum.CanMove:
                    s_currentTileOver.SetPlaceable(m_itemOnTile);
                    m_itemOnTile = null;
                    break;
                case TileStateEnum.CanAttack:
                    //TODO
                    // Find Tile that can be placed at this location.
                    if (s_currentTileOver.m_itemOnTile != null) {
                        Tile canAttackFrom = m_parentGrid.GetAccessibleAttackPosition(this, s_currentTileOver);
                        if(canAttackFrom != this) {
                            canAttackFrom.SetPlaceable(m_itemOnTile);
                            m_itemOnTile = null;
                        }
                        else {
                            SetPlaceable(m_itemOnTile);
                            turn_finished = false;
                        }
                        break;
                    }
                    else {
                        SetPlaceable(m_itemOnTile);
                        turn_finished = false;
                    }
                    break;
                case TileStateEnum.CanNotAccess:
                default:
                    SetPlaceable(m_itemOnTile);
                    turn_finished = false;
                    break;
            }

        }
        m_parentGrid.ClearPathableTiles();
        if (turn_finished) {
            GameManager.GetInstance<GameManager>().UpdateTurn();
        }
        successfullyGrabbed = false;
    }

    public void SetParentGrid(Grid parentGrid) {
        m_parentGrid = parentGrid;
    }

    public void SetPlaceable(IPlaceable toSet){
        m_itemOnTile = toSet;
        toSet.GetGameObject().transform.position = transform.position;
        toSet.GetGameObject().transform.SetParent(transform);
    }
    public IPlaceable GetPlaceable() {
        return m_itemOnTile;
    }



}