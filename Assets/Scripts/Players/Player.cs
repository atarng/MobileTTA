using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;
//namespace AtRng.MobileTTA {

public class Player : BasePlayer {

    private GameUnit m_nexus = null;
    
    int m_drawCost = 1;
    public override int DrawCost {
        get { return m_drawCost; }
        protected set {//private 
            m_drawCost = value;
            m_drawCost_ui.text = m_drawCost.ToString();

            m_deckCount_ui.text = DeckSize().ToString();
        }
    }

    // UI Elements
    //public SpriteRenderer m_actionPointPrefab;
    [SerializeField] private Text m_drawCost_ui;
    [SerializeField] private Text m_deckCount_ui;

    private bool m_deckPopulated = false;

    public override void PlaceUnitOnField(IUnit unitToPlace) {
        //unitToPlace.GetGameObject().transform.rotation = Quaternion.identity;
        m_hand.Remove(unitToPlace);
        RepositionCardsInHand();

        // order matters.
        // -1 because of nexus.
        // placing above addition of unit because I am using AttemptRelease.
        GameUnit candidateNexus = unitToPlace as GameUnit;
        if (candidateNexus != null && candidateNexus.IsNexus()) {
            m_nexus = candidateNexus;
            Health = m_nexus.GetPhysicalHealth();
        }
        else if (candidateNexus.IsMilitia()) {
            ActionPoints -= 2; // - 1 because of Attempt Release deducting an action point already.
        }
        else {
            ActionPoints -= (m_fieldUnits.Count - (m_nexus != null ? 1 : 0));
        }

        // order matters
        m_fieldUnits.Add(unitToPlace);
    }
    //UnitManager.UnitDesciption
    public void PopulateAndShuffleDeck<T>(List<T> deck_to_populate_with) where T : UnitManager.UnitDesciption {
        List<T> to_copy = new List<T>();
        to_copy.AddRange(deck_to_populate_with);
        while (to_copy.Count > 0) {
            int random = (int)(UnityEngine.Random.Range(0, to_copy.Count));
            m_deck.Add(to_copy[random]);
            to_copy.RemoveAt(random);
        }
        for (int i = 0; i < 3; i++) {
            Draw();
        }
        m_deckPopulated = true;
    }

    public override void UpdatePlayerHealth(int playerHealth) {
        base.UpdatePlayerHealth(playerHealth);
        //Debug.Log("[Player/UpdatePlayerHealth] Health: " + playerHealth);

        if (m_nexus.GetPhysicalHealth() != playerHealth) {
            int difference = m_nexus.GetPhysicalHealth() - playerHealth;
            m_nexus.TakeDamage(difference, 0);
        }
    }


    public void AttemptToDraw() {
        if (!SingletonMB.GetInstance<GameManager>().CurrentPlayer().Equals(this)) {
            //Debug.LogWarning("[Player/AttemptToDraw] Incorrect player turn");
            SceneControl.GetCurrentSceneControl().DisplayWarning("It is not that player's turn!");
        }
        else if (!GetEnoughActionPoints(DrawCost) || m_hand.Count >= 5) {
            //Debug.LogWarning("[Player/AttemptToDraw] At Max Hand Size or not enough action points.");
            SceneControl.GetCurrentSceneControl().DisplayWarning("At Max Hand Size or not enough action points.");
        }
        else if (m_deck.Count == 0) {
            SceneControl.GetCurrentSceneControl().DisplayWarning("You are out of tiles.");
        }
        else {
            if (m_deckPopulated) {
                Draw();
                ActionPoints -= DrawCost;
                DrawCost++;
            }
            /*
            else {
                //int[] dummyDeckList = new int[10];
                UnitManager um = SingletonMB.GetInstance<UnitManager>();
                List<UnitManager.UnitDefinition> to_insert = new List<UnitManager.UnitDefinition>();
                for (int i = 0; i < 10; i++) {//dummyDeckList.Length; i++) {
                    UnitManager.UnitDefinition ud = um.GetDefinition( (int)UnityEngine.Random.Range(1, 6) );//dummyDeckList[i]);
                    if (ud != null) {
                        to_insert.Add(ud);
                    }
                }
                PopulateDeck(to_insert);

                for (int j = 0; j < 3; j++) {
                    Draw();
                }
            }
            */
        }
    }

    public override void Draw() {
        if(m_deck.Count > 0 && m_hand.Count < 5) {
            // instantiate as a card
            UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition( m_deck[0].DefinitionID );
            GameUnit u = GameObject.Instantiate( SingletonMB.GetInstance<GameManager>().m_unitPrefab );
            u.ReadDefinition(ud);
            u.transform.SetParent(transform);

            u.transform.localPosition = Vector3.zero;
            u.transform.localRotation = Quaternion.identity;
            u.transform.localScale = Vector3.one * .01f;

            IUnit iu = u;
            iu.AssignPlayerOwner(ID);

            m_hand.Add(u);

            // TODO: Reposition "Cards"
            RepositionCardsInHand();

            m_deck.RemoveAt(0);
        }
        else{
            Debug.LogWarning("No more cards in deck.");
        }
    }


    public override bool GetEnoughActionPoints(int cost) {
        if (m_actionPoints < cost) {

            //Debug.LogWarning(string.Format("[Player/GetEnoughActionPoints] (Cost, Current, Total): ({0}, {1}, {2})",
            //    cost, m_actionPoints, m_actionPointsMax));
            SceneControl.GetCurrentSceneControl().DisplayWarning("Not Enough Action Points.");

            return false;
        }
        return true;
    }

    // This behavior might be able to be moved back into base player.
    // We need to determine whether or not base player should be considered
    // as something that should be a part of the NameSpace Package.
    public override bool CheckIfLost() {
        bool hasLost = base.CheckIfLost();
        if (!hasLost) {
            if(HandSize() == 0 && DeckSize() == 0 && GetCurrentSummonedUnits().Count == 1) {
                GameUnit u = GetCurrentSummonedUnits()[0] as GameUnit;
                // they actually lost all their units and only have their nexus left.
                if (u.IsNexus()) return true;
            }

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
    //

    // Drawing costs, etc.
    public override void BeginTurn() {
        base.BeginTurn();
    }
    // End of Turn
    // Might not be needed anymore.
    public override void EndTurn() {
        base.EndTurn();
    }


}