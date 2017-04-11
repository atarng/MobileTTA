/***
 * IGamePlayer: An Interface that describes the behavior we expect a player
 *              to be able to do on a grid based game.
 *              
 * Author: Alfred Tarng
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AtRng.MobileTTA {
    public interface IGamePlayer {

        void Draw();
        void RepositionCardsInHand();

        List<IUnit> GetCurrentSummonedUnits();
        void PlaceUnitOnField(IUnit unitToPlace);

        bool GetEnoughActionPoints(int cost);
        void ExpendUnitActionPoint();

        void BeginTurn();
        void EndTurn();

        void UpdatePlayerHealth(int playerHealth);
        bool CheckIfLost();

    }
}
