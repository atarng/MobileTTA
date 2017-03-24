using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;
using System;

public class Impassable : MonoBehaviour, IPlaceable {
    Tile m_assignedToTile = null;
    Tile IPlaceable.AssignedToTile {
        get {
            return m_assignedToTile;
        }
        set {
            m_assignedToTile = value;
            //m_assignedToTile.gameObject.SetActive(false);
        }
    }

    bool IPlaceable.AttemptRelease(bool resolved) {
        return false;
    }

    bool IPlaceable.AttemptSelection() {
        return false;
    }

    GameObject IPlaceable.GetGameObject() {
        return gameObject;
    }
}
