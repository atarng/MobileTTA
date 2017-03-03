using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AtRng.MobileTTA;

public class Tile : MonoBehaviour {

    IPlaceable   m_itemOnTile;
    static Tile s_currentTileOver;
    Grid m_parentGrid;

    public SpriteRenderer sr;

    public int xPos { get; set; }
    public int yPos { get; set; }

    private void OnMouseDown() {
//      Debug.Log("[Tile] OnMouseDown: " + name);
        if (m_itemOnTile != null) {

            m_itemOnTile.SetDragging();

            Unit u = m_itemOnTile.GetGameObject().GetComponent<Unit>();
            if (u != null) {
                m_parentGrid.DeterminePathableTiles(this, u);
            }
        }
    }
/*
    private void OnMouseOver(){
        Debug.Log("[Tile] OnMouseOver: " + name);
    }
//*/

    private void OnMouseEnter(){
        //Debug.Log("[Tile] OnMouseEnter: " + name);
        s_currentTileOver = this;
    }
    private void OnMouseExit() {
        //Debug.Log("[Tile] OnMouseExit: " + name);
        if(s_currentTileOver == this) {
            s_currentTileOver = null;
        }
    }
// */

    private void OnMouseUp(){

        Debug.Log("[Tile] OnMouseUp: " + name);

        // ray cast doesn't work with Collider 2d.


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
                        }
                        break;
                    }
                    else {
                        SetPlaceable(m_itemOnTile);
                    }
                    break;
                case TileStateEnum.CanNotAccess:
                default:
                    SetPlaceable(m_itemOnTile);
                    break;
            }
        }
        m_parentGrid.ClearPathableTiles();

    }

    public void SetParentGrid(Grid parentGrid) {
        m_parentGrid = parentGrid;
    }

    public void SetPlaceable(IPlaceable toSet){
        m_itemOnTile = toSet;
        toSet.GetGameObject().transform.position = transform.position;
    }
    public IPlaceable GetPlaceable() {
        return m_itemOnTile;
    }



}