using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;


public class Tile : MonoBehaviour {

    IPlaceable m_itemOnTile;

    [SerializeField]
    private Sprite[] m_spriteArray;

    [SerializeField]
    private Image m_spriteRenderer;
    public Image Sprite {
        get { return m_spriteRenderer; }
    }
    // TODO: FIX THIS MASK LOGIC
    // Mask for tile Movement
    TileTraversalEnum m_tte = TileTraversalEnum.All;
    public void SetTileTraversal(TileTraversalEnum tte) {
        m_tte = tte;
        switch (m_tte) {
            case TileTraversalEnum.CanFly:
                m_spriteRenderer.sprite = m_spriteArray[1];
                break;
            //case TileTraversalEnum.CanClimb:
            case TileTraversalEnum.FlyAndClimb:
                m_spriteRenderer.sprite = m_spriteArray[2];
                break;
            default:
                //m_spriteRenderer.sprite = m_spriteArray[0];
                break;
        }
    }
    public bool CanTraverse(TileTraversalEnum targetTTE) {
        return ((m_tte & targetTTE) > TileTraversalEnum.None);
    }
    // "Mountain"
    public int TraversalCost {
        get {
            return m_tte == TileTraversalEnum.FlyAndClimb ? 2 : 1;
            //return 1;
        }
    }

    public int xPos { get; set; }
    public int yPos { get; set; }

    public bool IsOccupied() {
        return m_itemOnTile != null;
    }

    public bool MatchesTilePredicate(Tile t) {
        return t == this;
    }

    /*
    Grid       m_parentGrid;
    public void SetParentGrid(Grid parentGrid) {
        m_parentGrid = parentGrid;
    }
    */
    public void SetPlaceable(IPlaceable toSet, bool assign = true){
        if (assign) {
            m_itemOnTile = toSet;
        }
        if (toSet != null) {
            toSet.GetGameObject().transform.SetParent(transform);
            Vector3 placement = transform.position;
            placement.z -= 1;
            toSet.GetGameObject().transform.position = placement;

            //toSet.AssignedToTile = this; // atarng: change to be based on unit instead of tile.
        }
    }
    public IPlaceable GetPlaceable() {
        return m_itemOnTile;
    }

}