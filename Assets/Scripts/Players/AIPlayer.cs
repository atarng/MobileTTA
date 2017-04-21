using AtRng.MobileTTA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIPlayer : BasePlayer {
    public override void Draw() {
        //throw new NotImplementedException();
        UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(m_deck[0].DefinitionID);

        AIUnit gu = GameObject.Instantiate(SingletonMB.GetInstance<GameManager>().m_AIUnitPrefab);
        gu.ReadDefinition(ud);
        gu.transform.SetParent(transform);

        gu.transform.localPosition = Vector3.zero;
        gu.transform.localRotation = Quaternion.identity;
        gu.transform.localScale    = Vector3.one * .01f;

        gu.AssignPlayerOwner(ID);

        m_hand.Add(gu);

        ActionPoints -= DrawCost;
        DrawCost += 1;

        Debug.Log("[AIPlayer/Draw] unit: " + ((gu != null) ? gu.name : "not implemented yet"));

        RepositionCardsInHand();

        m_deck.RemoveAt(0);
    }


    public override bool GetEnoughActionPoints(int cost) {
        return (m_actionPoints >= cost);
    }

    public override void PlaceUnitOnField(IUnit unitToPlace) {

        GameUnit candidateNexus = unitToPlace as GameUnit;
        if (candidateNexus != null && candidateNexus.IsNexus()) {
            m_fieldUnits.Add(unitToPlace);
            Health = candidateNexus.GetPhysicalHealth();
        }
        else {
            Debug.Log("[AIPlayer/PlaceUnitOnField] unit: " + unitToPlace.GetGameObject().name);
            for (int i = 0; i < m_fieldUnits.Count; i++) {
                List<Tile> tileList = GameManager.GetInstance<GameManager>().GetGrid().GetCircumference(m_fieldUnits[i].AssignedToTile, 1);
                for (int j = 0; j < tileList.Count; j++) {
                    if (tileList[j].GetPlaceable() == null) {

                        m_hand.Remove(unitToPlace);
                        RepositionCardsInHand();

                        unitToPlace.AssignedToTile = tileList[j];
                        ActionPoints -= m_fieldUnits.Count;
                        m_fieldUnits.Add(unitToPlace);

                        return;
                    }
                }
            }
        }
        
    }

    private void PerformUnitAction(IUnit unitToMove) { //, Tile tileToMoveTo, Tile toInteractWith) {
        Tile targetTile = null;
        Tile interactTile = null;
        AIUnit aiUnit = unitToMove as AIUnit;
        if (aiUnit != null) {
            aiUnit.DetermineTargetTiles(out targetTile, out interactTile);
            if (targetTile != null) {
                aiUnit.AssignedToTile = targetTile;
                ActionPoints -= 1;

                //
                if (interactTile != null) {
                    ICombat icp = interactTile.GetPlaceable() as ICombat;
                    if (icp != null) {
                        int dist = Mathf.Abs(aiUnit.AssignedToTile.xPos - icp.AssignedToTile.xPos) +
                                   Mathf.Abs(aiUnit.AssignedToTile.yPos - icp.AssignedToTile.yPos);
                        if (aiUnit.GetAttackRange() == dist) {
                            SingletonMB.GetInstance<GameManager>().HandleCombat(aiUnit, icp);
                        }
                    }
                    Debug.Log(string.Format("[AIPlayer/PerformUnitAction] unit_tile: {0}, tile_move: {1}, tile_interact: {2}",
                        unitToMove.AssignedToTile.name,
                        targetTile.name,
                        interactTile.name
                    ));
                }
                //else {
                //    Debug.LogError(string.Format("[AIPlayer/PerformUnitAction] unit_tile: {0}, tile_move: {1}",
                //        unitToMove.AssignedToTile.name,
                //        targetTile.name
                //    ));
                //}
            }
            else {
                Debug.LogWarning(string.Format("[AIPlayer/PerformUnitAction] unit_tile: {0}", unitToMove.AssignedToTile.name));
            }
        }
        else {
            // We Have A Problem
            // Well Actually this could be the nexus.
            GameUnit gu = unitToMove as GameUnit;
            if (gu != null && !gu.IsNexus()) {
                Debug.LogError("[AIPlayer] Attempting to Use Unit not as AIUnit");
            }
        }
    }

    public override void BeginTurn() {
        base.BeginTurn();

        // AI Needs to determine moves to do.
        // Check to see if it can kill a unit.
        // Check to see if it can attack a unit.
        // Move towards a Unit or Nexus.

        for (int i = 0; i < m_fieldUnits.Count && GetEnoughActionPoints(1); i++) {
            PerformUnitAction(m_fieldUnits[i]);//, targetTile, toInteractWith);
        }

        // Draw Cards
        // Place Units if enough action points left.
        int safeguard = 0;
        while (true) {

            if (safeguard++ > 100) {
                Debug.LogError("AI NOT READY.");
                break;
            }

            if (HandSize() > 0 && ActionPoints >= m_fieldUnits.Count) {
                // Can play a unit
                IUnit iu = m_hand[0];
                PlaceUnitOnField(iu);
            }
            else if (DeckSize() > 0 && ActionPoints >= DrawCost && HandSize() < 5) {
                Draw();
            }
            else {
                //
                Debug.Log("No More Actions to take");
                EndTurn();
                break;
            }

        }

    }
}