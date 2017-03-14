using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace AtRng.MobileTTA {
    public interface ICombatPlaceable : IPlaceable {        
        void TakeDamage(int damage, int type);
    }
}