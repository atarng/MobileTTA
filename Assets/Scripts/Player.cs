using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AtRng.MobileTTA {

    public class Player : MonoBehaviour, IGamePlayer { //, IPlaceable {

        int m_health = 0;
        int m_actionPoints = 0;
        int ActionPoints {
            get {
                return m_actionPoints;
            }
            set {
                m_actionPoints = value;
                if (m_actionPoints == 0) {
                    EndTurn();
                }
            }
        }

        int m_actionPointsMax = 0;
        int m_drawCost = 1;

        // UI Elements
        public Image m_actionPointPrefab;
        List<Image>  m_actionPointsUI;

        public List<UnitManager.UnitDefinition> m_deck = new List<UnitManager.UnitDefinition>();
        List<IUnit> m_hand = new List<IUnit>();
        List<IUnit> m_fieldUnits = new List<IUnit>();

        int IGamePlayer.GetCurrentDrawCost() {
            return m_drawCost;
        }
        public List<IUnit> GetCurrentSummonedUnits() {
            return m_fieldUnits;
        }

        public void PlaceUnitOnField(IUnit unitToPlace) {
            ActionPoints -= m_fieldUnits.Count;

            m_hand.Remove(unitToPlace);
            m_fieldUnits.Add(unitToPlace);
        }

        public void PopulateDeck(List<UnitManager.UnitDefinition> deck_to_populate_with) {
            List<UnitManager.UnitDefinition> to_copy = new List<UnitManager.UnitDefinition>();
            to_copy.AddRange( deck_to_populate_with );
            while (to_copy.Count > 0) {
                int random = (int)(UnityEngine.Random.Range(0, to_copy.Count));
                m_deck.Add(to_copy[random]);
                to_copy.RemoveAt(random);
            }
        }

        public void AttemptToDraw() {
            if (!SingletonMB.GetInstance<GameManager>().CurrentPlayer().Equals(this)) {
                Debug.LogWarning("[Player/AttemptToDraw] Incorrect player turn");
            }
            else if (!GetEnoughActionPoints(m_drawCost) || GetHand().Count >= 5) {
                Debug.LogWarning("[Player/AttemptToDraw] At Max Hand Size or not enough action points.");
            }
            else {
                Draw();
                ActionPoints -= m_drawCost;
                m_drawCost++;
            }
        }

        public void Draw() {
            if(m_deck.Count > 0) {

                // instantiate as a card
                UnitManager.UnitDefinition ud = m_deck[0];
                Unit u = GameObject.Instantiate( SingletonMB.GetInstance<GameManager>().m_unitPrefab );
                u.ReadDefinition(ud);
                u.transform.SetParent(transform);
                u.transform.localPosition = Vector3.zero;
                IUnit iu = u;
                iu.GenerateCardBehavior();
                iu.AssignPlayerOwner(this);

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

        // Drawing costs, etc.
        public void Reset() {
            m_actionPointsMax++;
            m_actionPoints = m_actionPointsMax;
            m_drawCost = 1;

            for (int i = 0; i < m_fieldUnits.Count; i++) {
                m_fieldUnits[i].Clear();
            }
        }
        int IGamePlayer.GetHealth() {
            return m_health;
        }
        public GameObject GetGameObject() {
            throw new NotImplementedException();
        }

        public bool GetEnoughActionPoints(int cost) {
            if (m_actionPoints < cost) {
                Debug.LogWarning(string.Format("[Player/GetEnoughActionPoints] (Cost, Current, Total): ({0}, {1}, {2})",
                    cost, m_actionPoints, m_actionPointsMax));
                return false;
            }
            return true;
        }
        void IGamePlayer.ExpendUnitActionPoint() {
            ActionPoints--;
        }

        public void EndTurn() {
            if(GameManager.GetInstance<GameManager>().CurrentPlayer().Equals(this)) {
                GameManager.GetInstance<GameManager>().UpdateTurn();
            }
        }
/*
        // These aren't really for player.
        bool IPlaceable.IsDragging() {
            throw new NotImplementedException();
        }

        bool IPlaceable.AttemptSelection() {
            return false;
        }

        bool IPlaceable.AttemptRelease(Tile sourceTile, Tile destinationTile) {
            throw new NotImplementedException();
        }
*/

    }
}