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

    public bool IsOccupied() {
        return m_itemOnTile != null;
    }

    public bool MatchesTile(Tile t) {
        return t == this;
    }

    public void SetParentGrid(Grid parentGrid) {
        m_parentGrid = parentGrid;
    }

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