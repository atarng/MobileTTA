using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {

    public interface IUnit {

        IGamePlayer GetPlayerOwner();
        void AssignPlayerOwner(int playerID);

        int GetPhysicalHealth();
        void ModifyPhysicalHealth(int amount);

        int GetSpiritualHealth();
        void ModifySpiritualHealth(int amount);

        int GetAttackValue();
        // int GetAttackType();
        bool IsSpiritualAttack();
        bool IsPhysicalAttack();
        int GetAttackRange();

        // int GetMovementType();
        int GetMaxMovement();
        bool HasPerformedAction();
        bool Clear();

        // Progression Accessors

    }
}