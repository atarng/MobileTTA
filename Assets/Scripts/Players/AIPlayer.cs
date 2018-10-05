using AtRng.MobileTTA;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIPlayer : BasePlayer {
    Queue<Action> m_actionQueue = new Queue<Action>();

    const int MILITIA_COST = 3;
    const int MILITIA_ID = 10;

    private AIUnit GenerateAIUnit(int defID) {
        UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition(defID);

        AIUnit generatedUnit = GameObject.Instantiate(SingletonMB.GetInstance<GameManager>().m_AIUnitPrefab);
        generatedUnit.AssignPlayerOwner(ID);
        generatedUnit.ReadDefinition(ud);
        generatedUnit.transform.SetParent((ID % 2 > 0) ? m_handTransform : transform);
        //generatedUnit.transform.SetParent(transform);

        generatedUnit.transform.localPosition = Vector3.zero;
        generatedUnit.transform.localRotation = Quaternion.identity;
        generatedUnit.transform.localScale = Vector3.one;// * .01f;

        return generatedUnit;
    }

    /***
     * 
     */
    public override void Draw() {
        AIUnit gu = GenerateAIUnit(m_deck[0].DefinitionID);

        m_hand.Add(gu);
        int oldDrawCost = DrawCost;
        DrawCost += 1;

        RepositionCardsInHand();
        m_deck.RemoveAt(0);

        ActionPoints -= oldDrawCost;
    }


    public override bool GetEnoughActionPoints(int cost) {
        return (m_actionPoints >= cost);
    }

    public override void PlaceUnitOnField(IUnit unitToPlace) {
        GameManager gm = SingletonMB.GetInstance<GameManager>();
        AudioManager am = SingletonMB.GetInstance<AudioManager>();
        GameUnit candidateNexus = unitToPlace as GameUnit;
        if (candidateNexus != null && candidateNexus.IsNexus()) {
            m_fieldUnits.Add(unitToPlace);
            Health = candidateNexus.GetPhysicalHealth();
            m_nexus = candidateNexus;
        }
        else {
            //Debug.Log("[AIPlayer/PlaceUnitOnField] unit: " + unitToPlace.GetGameObject().name);
            for (int i = 0; i < m_fieldUnits.Count; i++) {
                List<Tile> tileList = gm.GetGrid().GetCircumference(m_fieldUnits[i].AssignedToTile, 1);
                for (int j = 0; j < tileList.Count; j++) {
                    if (!tileList[j].IsOccupied()) {
                        unitToPlace.AssignedToTile = tileList[j];
                        m_fieldUnits.Add(unitToPlace);
                        ActionPoints -= ((unitToPlace.GetID() == MILITIA_ID) ? MILITIA_COST : m_fieldUnits.Count);

                        am.PlaySound("Tile");

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
        GameManager gm = SingletonMB.GetInstance<GameManager>();
        AudioManager am = SingletonMB.GetInstance<AudioManager>();
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
                            gm.HandleCombat(aiUnit, icp);
                        }
                    }
                    Debug.Log(string.Format("[AIPlayer/PerformUnitAction] unit_tile: {0}, tile_move: {1}, tile_interact: {2}",
                        unitToMove.AssignedToTile.name,
                        targetTile.name,
                        interactTile.name
                    ));
                }
                am.PlaySound("Tile");
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

    IEnumerator UnqueueAIActions() {
        yield return new WaitForSeconds(2);

        while (m_actionQueue.Count > 0) {
            Action toExecute = m_actionQueue.Dequeue();
            if (toExecute != null) {
                toExecute();
            }
            yield return new WaitForSeconds(1);
        }
    }

    public override void BeginTurn() {
        base.BeginTurn();

        int remainingActionPoints = ActionPoints;
        int tempDeckSize = DeckSize();
        int fieldUnitCost = m_fieldUnits.Count;
        int remainingHealth = Health;
        // AI Needs to determine moves to do.
        // Check to see if it can kill a unit.
        // Check to see if it can attack a unit.
        // Move towards a Unit or Nexus.
        //GetEnoughActionPoints(1)
        for (int i = 0; i < m_fieldUnits.Count && remainingActionPoints > 0; i++) {
            //PerformUnitAction(m_fieldUnits[i]);
            IUnit iu = m_fieldUnits[i];
            AIUnit aiUnit = iu as AIUnit;
            if(aiUnit != null && !aiUnit.IsNexus()) {
                m_actionQueue.Enqueue(() => { PerformUnitAction(iu); });
                remainingActionPoints -= 1;
            }
        }

        // Draw Cards
        // Place Units if enough action points left.
        int safeguard = 0;

        while (true) {

            if (safeguard++ > 100) {
                Debug.LogError("AI NOT READY.");
                break;
            }

            if (HandSize() > 0 && remainingActionPoints >= fieldUnitCost) {
                // Can play a unit
                IUnit iu = m_hand[0];
                m_actionQueue.Enqueue(() => {
                    PlaceUnitOnField(iu);
                    RepositionCardsInHand();
                });
                m_hand.Remove(iu);
                remainingActionPoints -= fieldUnitCost++;
            }
            else if (tempDeckSize > 0 && HandSize() < 5
                && remainingActionPoints >= DrawCost
            ) {
                m_actionQueue.Enqueue(() => { Draw(); });

                remainingActionPoints -= DrawCost++;
                tempDeckSize--;
            }
            else if (remainingActionPoints >= MILITIA_COST && remainingHealth > 10) {
                // place a militia on the field
                m_actionQueue.Enqueue(() => {
                    AIUnit aiu = GenerateAIUnit(MILITIA_ID);
                    PlaceUnitOnField(aiu);
                    Health -= 10;
                    UpdatePlayerHealth(Health);
                });
                remainingActionPoints -= MILITIA_COST;
                remainingHealth       -= 10;
            }
            else {
                // Debug.Log("No More Actions to take");
                m_actionQueue.Enqueue(() => { EndTurn(); });
                break;
            }

        }

        DrawCost = 1;
        StartCoroutine(UnqueueAIActions());

    }

/*** VERIFY LATER IF THIS SHOULD BE IN BASE CLASS OR IF AI NEEDS ITS OWN IMPLEMENTATION ***/

    private GameUnit m_nexus = null;
    public override void UpdatePlayerHealth(int playerHealth) {
        base.UpdatePlayerHealth(playerHealth);
        if (m_nexus.GetPhysicalHealth() != playerHealth) {
            int difference = m_nexus.GetPhysicalHealth() - playerHealth;
            m_nexus.TakeDamage(difference, 0);
        }
    }
    public override bool CheckIfLost() {
        bool hasLost = base.CheckIfLost();
        if (!hasLost) {
            /*
            if (HandSize() == 0 && DeckSize() == 0 && GetCurrentSummonedUnits().Count == 1) {
                GameUnit u = GetCurrentSummonedUnits()[0] as GameUnit;
                // they actually lost all their units and only have their nexus left.
                if (u.IsNexus()) return true;
            }
            */
            // OR Nexus is surrounded.
            if (m_nexus != null) {
                List<Tile> lt = GameManager.GetInstance<GameManager>().GetGrid().GetCircumference(m_nexus.AssignedToTile, 1);
                for (int i = 0; i < lt.Count; i++) {
                    IPlaceable ip = lt[i].GetPlaceable();
                    if (ip == null) return false;
                    else {
                        GameUnit u = ip as GameUnit;
                        if (u != null && this.Equals(u.GetPlayerOwner())) {
                            return false;
                        }
                    }
                }
                // no open spots and they are occupied by enemy units.
                hasLost = true;
            }
        }
        return hasLost;
    }
}