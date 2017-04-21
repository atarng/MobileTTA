using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {
    public interface IUnit : IPlaceable {
        int GetID();

        TileTraversalEnum CanTraverse { get; }
        bool HasPerformedAction { get; }

        IGamePlayer GetPlayerOwner();
        void AssignPlayerOwner(int playerID);

        int GetPhysicalHealth();
        int GetSpiritualHealth();

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