using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System;

namespace AtRng.MobileTTA {
    public abstract class BasePlayer : MonoBehaviour, IGamePlayer {

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
        public abstract int Health { get; protected set; }
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

        public abstract void Draw();
        public abstract int DrawCost { get; protected set; }

        void IGamePlayer.ExpendUnitActionPoint() {
            ActionPoints--;
        }
        public abstract List<IUnit> GetCurrentSummonedUnits();
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
                DrawCost = SingletonMB.GetInstance<GameManager>().DrawMode() ? 2 : 1;
            }
/*
            else if (m_fieldUnits.Count > 0) {
                // if is nexus
                Unit ifNexus = m_fieldUnits[0] as Unit;
                if (ifNexus.IsNexus()) {
                    m_fieldUnits[0].TakeDamage(10, 0);
                }
                DrawCost = 0;
            }
*/
        }
        public virtual void EndTurn() {
            for (int i = 0; i < m_fieldUnits.Count; i++) {
                m_fieldUnits[i].ClearStates();
            }
            if (GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(this)) {
                GameManager.GetInstance<GameManager>().UpdateTurn();
            }
        }
    }
}