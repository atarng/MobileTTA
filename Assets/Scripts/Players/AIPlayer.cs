using AtRng.MobileTTA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIPlayer : BasePlayer {
    public override int DrawCost {
        get {
            throw new NotImplementedException();
        }
        protected set {
            throw new NotImplementedException();
        }
    }

    public override void Draw() {
        throw new NotImplementedException();
    }

    public override List<IUnit> GetCurrentSummonedUnits() {
        throw new NotImplementedException();
    }

    public override bool GetEnoughActionPoints(int cost) {
        throw new NotImplementedException();
    }

    public override int Health {
        get {
            return 0;
        }
        protected set {

        }
    }

    public override void PlaceUnitOnField(IUnit unitToPlace) {
        throw new NotImplementedException();
    }
    public override void BeginTurn() {
        base.BeginTurn();

        // AI Needs to determine moves to do.
        // Check to see if it can kill a unit.
        // Check to see if it can attack a unit.
        // Move towards a Unit or Nexus.
        // Place Units if enough action points left.
        // Draw Cards
    }
}