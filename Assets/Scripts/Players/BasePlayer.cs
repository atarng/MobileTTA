using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using System;

namespace AtRng.MobileTTA {
    public abstract class BasePlayer : MonoBehaviour, IGamePlayer {

        private int m_playerHealth = 100;
        public virtual int Health {
            get {
                return m_playerHealth;
            }
            protected set {
                m_playerHealth = value;
            }
        }

        protected bool m_deckPopulated = false;

        protected int m_actionPointsMax = 0;
        protected int m_actionPoints = 0;
        protected List<SpriteRenderer> m_actionPointsUI = new List<SpriteRenderer>();

        protected List<IUnit> m_hand       = new List<IUnit>();
        protected List<IUnit> m_fieldUnits = new List<IUnit>();
        protected List<UnitManager.UnitDesciption> m_deck = new List<UnitManager.UnitDesciption>();

        public int HandSize() {
            return m_hand.Count;
        }
        public int DeckSize() {
            return m_deck.Count;
        }

        public int ID { get; set; }
        public int ActionPoints {
            get {
                return m_actionPoints;
            }
            protected set {
                m_actionPoints = value;
                // Color
                for (int i = 0; i < m_actionPointsUI.Count; i++) {
                    m_actionPointsUI[i].color = (i < m_actionPoints) ? Color.blue : Color.grey;
                }
                if (m_actionPoints <= 0) {
                    EndTurn();
                }
            }
        }

        public List<IUnit> GetCurrentSummonedUnits() {
            return m_fieldUnits;
        }

        int m_drawCost = 1;
        public int DrawCost {
            get { return m_drawCost; }
            protected set {//private 
                m_drawCost = value;
                m_drawCost_ui.text = m_drawCost.ToString();
                m_deckCount_ui.text = DeckSize().ToString();
            }
        }

        // UI Elements
        //public SpriteRenderer m_actionPointPrefab;
        [SerializeField]
        private Text m_drawCost_ui;
        [SerializeField]
        private Text m_deckCount_ui;

///////////////

        public abstract void Draw();
        //public abstract int DrawCost { get; protected set; }

        public virtual bool CheckIfLost() {
            bool hasLost = false;

            // if lost all playable units.
            if ((HandSize() == 0 && DeckSize() == 0)) {
                hasLost = GetCurrentSummonedUnits().Count == 0;
            }

            if (!hasLost) {
                hasLost = Health == 0;
            }

            return hasLost;
        }

        public virtual void UpdatePlayerHealth(int playerHealth) {
            Health = playerHealth;
            CheckIfLost();
        }

        void IGamePlayer.ExpendUnitActionPoint() {
            ActionPoints--;
        }

        public abstract bool GetEnoughActionPoints(int cost);

        public abstract void PlaceUnitOnField(IUnit unitToPlace);

        public void RepositionCardsInHand() {
            for (int i = 0; i < m_hand.Count; i++) {
                Vector3 v3 = m_hand[i].GetGameObject().transform.localPosition;
                v3.x = (i - (m_hand.Count / 2)) - 0.5f;
                v3.y = 0;
                m_hand[i].GetGameObject().transform.localPosition = v3;
            }
        }

        public virtual void BeginTurn() {
            //Debug.Log(string.Format("[BasePlayer/{0}] BeginTurn", name));
            // Add ActionPoint: Limit to ten.
            m_actionPointsMax = Mathf.Min(10, m_actionPointsMax + 1);
            m_actionPoints = m_actionPointsMax;

            // Instantiate action point ui.
            for (int i = m_actionPointsUI.Count; i < m_actionPointsMax; i++) {
                SpriteRenderer sr = GameObject.Instantiate<SpriteRenderer>(SingletonMB.GetInstance<GameManager>().GetActionPointSprite());
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

            // Reset Draw Cost if Deck Still Has Cards*.
            if (m_deck.Count > 0) {
                //DrawCost = SingletonMB.GetInstance<GameManager>().DrawMode() ? 2 : 1;
                DrawCost = 1;
            }
        }

        public virtual void EndTurn() {
            for (int i = 0; i < m_fieldUnits.Count; i++) {
                m_fieldUnits[i].ClearStates();
            }
            if (GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(this)) {
                GameManager.GetInstance<GameManager>().UpdateTurn();
            }
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

    }
}