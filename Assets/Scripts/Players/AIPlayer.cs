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


    public override bool GetEnoughActionPoints(int cost) {
        throw new NotImplementedException();
    }

    public override void PlaceUnitOnField(IUnit unitToPlace) {
        throw new NotImplementedException();
    }

    private void MoveUnit(IUnit unitToMove, Tile tileToMoveTo) {//Tile toInteractWith) {
    }

    public override void BeginTurn() {
        base.BeginTurn();

        // AI Needs to determine moves to do.
        // Check to see if it can kill a unit.
        // Check to see if it can attack a unit.
        // Move towards a Unit or Nexus.
        // Place Units if enough action points left.
        // Draw Cards

        for (int i = 0; i < m_fieldUnits.Count; i++) {
            Tile targetTile = null;
            MoveUnit(m_fieldUnits[i], targetTile);
        }
    }
}