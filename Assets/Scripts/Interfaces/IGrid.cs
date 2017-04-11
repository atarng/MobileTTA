using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtRng.MobileTTA {
    interface IGrid {
        void InitializeGrid(int width, int height);
        void ClearGrid();
    }
}
