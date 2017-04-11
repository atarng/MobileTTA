﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace AtRng.MobileTTA {
    public interface ICombat {
        void TakeDamage(int damage, int type);

        bool IsAlive();
    }
}