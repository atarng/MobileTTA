using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AtRng.MobileTTA {
    public interface IGamePlayer {

        List<IUnit> GetCurrentSummonedUnits();

        int GetCurrentDrawCost();
        //int GetHandSize();
        List<IUnit> GetHand();
    }
}
