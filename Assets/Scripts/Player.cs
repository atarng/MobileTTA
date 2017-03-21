using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AtRng.MobileTTA {

    public class Player : MonoBehaviour, IGamePlayer { //, IPlaceable {
        public int ID { get; set; }
        int m_health = 0;
        int m_actionPoints = 0;
        int ActionPoints {
            get {
                return m_actionPoints;
            }
            set {
                m_actionPoints = value;

                for (int i = 0; i < m_actionPointsUI.Count; i++) {
                    // Color
                    m_actionPointsUI[i].color = (i < m_actionPoints) ? Color.blue : Color.grey;
                }

                if (m_actionPoints <= 0) {
                    EndTurn();
                }
            }
        }

        int m_actionPointsMax = 0;
        int m_drawCost = 1;
        public int DrawCost {
            get { return m_drawCost; }
            private set {
                m_drawCost = value;
                m_drawCost_ui.text = m_drawCost.ToString();
            }
        }

        // UI Elements
        public SpriteRenderer m_actionPointPrefab;
        List<SpriteRenderer>  m_actionPointsUI = new List<SpriteRenderer>();
        [SerializeField] private Text m_drawCost_ui;

        private bool m_deckPopulated = false;
        public List<UnitManager.UnitDesciption> m_deck = new List<UnitManager.UnitDesciption>();

        List<IUnit> m_hand = new List<IUnit>();
        List<IUnit> m_fieldUnits = new List<IUnit>();

        public List<IUnit> GetCurrentSummonedUnits() {
            return m_fieldUnits;
        }

        public void PlaceUnitOnField(IUnit unitToPlace) {
            //unitToPlace.GetGameObject().transform.rotation = Quaternion.identity;

            m_hand.Remove(unitToPlace);
            RepositionCardsInHand();
            m_fieldUnits.Add(unitToPlace);

            // order matters.
            ActionPoints -= (m_fieldUnits.Count - 1);
        }
        //UnitManager.UnitDesciption
        public void PopulateAndShuffleDeck<T>(List<T> deck_to_populate_with) where T : UnitManager.UnitDesciption {
            List<T> to_copy = new List<T>();
            to_copy.AddRange(deck_to_populate_with);
            /*
            for (int i = 0; i < deck_to_populate_with.Count; i++) {
                to_copy.Add( deck_to_populate_with[i] );
            }
            */
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

        public void AttemptToDraw() {
            if (!SingletonMB.GetInstance<GameManager>().CurrentPlayer().Equals(this)) {
                //Debug.LogWarning("[Player/AttemptToDraw] Incorrect player turn");
                SceneControl.GetCurrentSceneControl().DisplayWarning("It is not that player's turn!");
            }
            else if (!GetEnoughActionPoints(DrawCost) || GetHand().Count >= 5) {
                //Debug.LogWarning("[Player/AttemptToDraw] At Max Hand Size or not enough action points.");
                SceneControl.GetCurrentSceneControl().DisplayWarning("At Max Hand Size or not enough action points.");
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

        public void Draw() {
            if(m_deck.Count > 0) {

                // instantiate as a card
                UnitManager.UnitDefinition ud = UnitManager.GetInstance<UnitManager>().GetDefinition( m_deck[0].DefinitionID );
                Unit u = GameObject.Instantiate( SingletonMB.GetInstance<GameManager>().m_unitPrefab );
                u.ReadDefinition(ud);
                u.transform.SetParent(transform);
                u.transform.localPosition = Vector3.zero;

                u.transform.localRotation = Quaternion.identity;

                IUnit iu = u;
                iu.GenerateCardBehavior();
                iu.AssignPlayerOwner(ID);// this);

                /*
                // Rotate towards the center
                Vector3 lookAt = go.transform.position;
                lookAt.x = 0;
                Vector3 vectorToTarget = (lookAt - go.transform.transform.position).normalized;
                float angle = Mathf.Atan2(vectorToTarget.x, vectorToTarget.y) * Mathf.Rad2Deg;
                Quaternion q = Quaternion.AngleAxis(angle, Vector3.back);
                go.transform.rotation = q;
                */

                m_hand.Add(u);

                // TODO: Reposition "Cards"
                RepositionCardsInHand();

                m_deck.RemoveAt(0);
            }
            else{
                Debug.LogWarning("No more cards in deck.");
            }
        }

        public List<IUnit> GetHand() {
            return m_hand;
        }

        public void RepositionCardsInHand() {
            for (int i = 0; i < m_hand.Count; i++) {
                Vector3 v3 = m_hand[i].GetGameObject().transform.localPosition;
                v3.x = i - (m_hand.Count / 2);
                v3.y = 0;
                m_hand[i].GetGameObject().transform.localPosition = v3;
            }
        }

        int IGamePlayer.GetHealth() {
            return m_health;
        }

        public bool GetEnoughActionPoints(int cost) {
            if (m_actionPoints < cost) {

                //Debug.LogWarning(string.Format("[Player/GetEnoughActionPoints] (Cost, Current, Total): ({0}, {1}, {2})",
                //    cost, m_actionPoints, m_actionPointsMax));
                SceneControl.GetCurrentSceneControl().DisplayWarning("Not Enough Action Points.");

                return false;
            }
            return true;
        }
        void IGamePlayer.ExpendUnitActionPoint() {
            ActionPoints--;
        }

        // Drawing costs, etc.
        public void Reset() {
            // Limit to ten.
            m_actionPointsMax = Mathf.Min(10, m_actionPointsMax + 1);
            m_actionPoints = m_actionPointsMax;
            DrawCost = 1;

            for (int i = m_actionPointsUI.Count; i < m_actionPointsMax; i++) {
                SpriteRenderer sr = GameObject.Instantiate<SpriteRenderer>(m_actionPointPrefab);
                sr.transform.SetParent(transform);
                m_actionPointsUI.Add(sr);
            }
            for (int i = 0; i < m_actionPointsUI.Count; i++) {
                // Position
                Vector3 v3 = m_actionPointsUI[i].transform.localPosition;
                v3.x = -3 + (i * .5f);
                v3.y = 1;
                m_actionPointsUI[i].transform.localPosition = v3;

                // Color
                m_actionPointsUI[i].color = Color.blue;
            }
        }
        // End of Turn
        // Might not be needed anymore.
        public void EndTurn() {

            for (int i = 0; i < m_fieldUnits.Count; i++) {
                m_fieldUnits[i].Clear();
            }

            if (GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(this)) {
                GameManager.GetInstance<GameManager>().UpdateTurn();
            }
        }


    }
}