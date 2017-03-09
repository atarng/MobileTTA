using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AtRng.MobileTTA {
    public interface IGamePlayer {

        void Reset(); // Drawing costs, etc.

        int GetCurrentDrawCost();
        bool GetEnoughActionPoints(int cost);

        //int GetHandSize();
        List<IUnit> GetHand();
        List<IUnit> GetCurrentSummonedUnits();
        void PlaceUnitOnField(IUnit unitToPlace);
        void RepositionCardsInHand();
        void ExpendUnitActionPoint();

        int GetHealth();
    }
}
