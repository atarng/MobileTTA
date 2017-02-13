using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUnit {

    int  GetPhysicalHealth();
    void ModifyPhysicalHealth(int amount);

    int GetSpiritualHealth();
    void ModifySpiritualHealth(int amount);

    int GetAttackValue();
    // int GetAttackType();
    bool IsSpiritualAttack();
    bool IsPhysicalAttack();

    // int GetMovementType();
    int GetMaxMovement();
    bool HasPerformedAction();
    bool Clear();

    // Progression Accessors

}