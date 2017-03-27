using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace AtRng.MobileTTA {
    public interface IGamePlayer {

        //int DrawCost { get; }
        //List<IUnit> GetHand();

        void Draw();
        void RepositionCardsInHand();

        List<IUnit> GetCurrentSummonedUnits();
        void PlaceUnitOnField(IUnit unitToPlace);

        bool GetEnoughActionPoints(int cost);
        void ExpendUnitActionPoint();

        void BeginTurn();
        void EndTurn();

    }
}
