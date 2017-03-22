using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;
using System;

public class Impassable : MonoBehaviour, IPlaceable {

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    Tile IPlaceable.AssignedToTile {
        get {
            throw new NotImplementedException();
        }

        set {
            throw new NotImplementedException();
        }
    }

    bool IPlaceable.AttemptRelease(bool resolved) {
        throw new NotImplementedException();
    }

    bool IPlaceable.AttemptSelection() {
        throw new NotImplementedException();
    }

    GameObject IPlaceable.GetGameObject() {
        throw new NotImplementedException();
    }

}
