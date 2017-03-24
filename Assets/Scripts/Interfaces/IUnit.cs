﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {
    public interface IUnit : ICombatPlaceable {

        bool HasPerformedAction { get; }

        IGamePlayer GetPlayerOwner();
        void AssignPlayerOwner(int playerID);

        int GetPhysicalHealth();
        void ModifyPhysicalHealth(int amount);

        int GetSpiritualHealth();
        void ModifySpiritualHealth(int amount);

        int GetAttackValue();
        bool IsSpiritualAttack();
        bool IsPhysicalAttack();
        int GetAttackRange();

        int GetMaxMovement();
        bool ClearStates();

        //
        void GenerateCardBehavior();
        bool IsDragging();

    }
}