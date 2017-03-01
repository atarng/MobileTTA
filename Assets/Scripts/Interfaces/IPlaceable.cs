using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace AtRng.MobileTTA {
    public interface IPlaceable {

        bool IsDragging();
        void SetDragging();
        bool AttemptRelease(Tile sourceTile, Tile destinationTile);

        GameObject GetGameObject();

    }
}