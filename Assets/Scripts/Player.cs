using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {

    public class Player : MonoBehaviour, IGamePlayer { //, IPlaceable {

        int m_health = 0;
        int m_actionPoints = 0;

        private void OnMouseUp() {
            Debug.Log("[Player] OnMouseUp");
        }
        private void OnMouseEnter() {
            Debug.Log("[Player] OnMouseEnter");
        }
        private void OnMouseExit() {
            Debug.Log("[Player] OnMouseExit");
        }
        private void OnMouseDown() {
            Debug.Log("[Player] ClickedAsCard");
        }

        public List<IUnit> m_deck = new List<IUnit>();
        List<IUnit> m_hand = new List<IUnit>();

        int IGamePlayer.GetCurrentDrawCost() {
            throw new NotImplementedException();
        }

        public List<IUnit> GetCurrentSummonedUnits() {
            throw new NotImplementedException();
        }

        public void PopulateDeck(List<IUnit> deck_to_populate_with) {
            List<IUnit> to_copy = new List<IUnit>();
            to_copy.AddRange( deck_to_populate_with );
            while (to_copy.Count > 0) {
                int random = (int)(UnityEngine.Random.Range(0, to_copy.Count));
                m_deck.Add(to_copy[random]);
                to_copy.RemoveAt(random);
            }
        }

        public void AttemptToDraw() {
            if (GetHand().Count < 5) {
                Draw();
            }
        }

        public void Draw() {
            if(m_deck.Count > 0) {

                // instantiate as a card
                GameObject go = GameObject.Instantiate(m_deck[0].GetGameObject());
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;
                Unit u = go.GetComponent<Unit>();
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
                m_hand[i].GetGameObject().transform.localPosition = v3;
            }
        }

        // Drawing costs, etc.
        void IGamePlayer.Reset() {
            throw new NotImplementedException();
        }
        int IGamePlayer.GetHealth() {
            return m_health;
        }
        public GameObject GetGameObject() {
            throw new NotImplementedException();
        }

        public int GetActionPointsLeft() {
            throw new NotImplementedException();
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