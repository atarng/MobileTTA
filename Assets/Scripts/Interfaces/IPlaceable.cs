using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace AtRng.MobileTTA {
    public interface IPlaceable {

        bool AttemptSelection();
        bool AttemptRelease(bool resolved);

        GameObject GetGameObject();

        Tile AssignedToTile { get; set; }
    }
}