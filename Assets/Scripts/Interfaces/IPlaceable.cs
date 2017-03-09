using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace AtRng.MobileTTA {
    public interface IPlaceable {

        bool IsDragging();
        bool AttemptSelection();
        bool AttemptRelease(Tile sourceTile, Tile destinationTile, bool resolved);

        GameObject GetGameObject();
        
        void TakeDamage(int damage, int type);

        Tile AssignedToTile { get; set; }
    }
}