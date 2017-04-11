using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {
    public interface IUnit : IPlaceable {
        TileTraversalEnum CanTraverse { get; }
        bool HasPerformedAction { get; }

        IGamePlayer GetPlayerOwner();
        void AssignPlayerOwner(int playerID);

        int GetPhysicalHealth();
        int GetSpiritualHealth();
        //void ModifyPhysicalHealth(int amount);
        //void ModifySpiritualHealth(int amount);

        int GetAttackValue();
        bool IsSpiritualAttack();
        bool IsPhysicalAttack();
        int GetAttackRange();

        int GetMaxMovement();
        bool ClearStates();

        //
        //void GenerateCardBehavior();
        bool IsDragging();

    }
}