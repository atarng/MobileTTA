using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using AtRng.MobileTTA;
using System;

public class EditorUnit : BaseUnit {
    public override bool AttemptRelease(bool resolved) {
        throw new NotImplementedException();
    }

    public override bool AttemptSelection() {
        throw new NotImplementedException();
    }

    public override IGamePlayer GetPlayerOwner() {
        throw new NotImplementedException();
    }
}
