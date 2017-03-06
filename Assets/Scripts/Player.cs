using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {

    public class Player : MonoBehaviour, IGamePlayer { //, IPlaceable {

        int m_health = 0;
        int m_actionPoints = 0;

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

        public void Draw() {
            if(m_deck.Count > 0) {

                // instantiate as a card
                GameObject go = GameObject.Instantiate(m_deck[0].GetGameObject());
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;

                // Attempted to rotate but forgot ui is based on a differnt object.

                //Vector3 lookAt = go.transform.position;
                //lookAt.x = 0;
                //go.transform.LookAt(lookAt);

                m_hand.Add( go.GetComponent<Unit>() );

                // TODO: Reposition "Cards"
                RepositionCardsInHand();

                m_deck.RemoveAt(0);
            }
        }

        public List<IUnit> GetHand() {
            return m_hand;
        }

        private void RepositionCardsInHand() {
            for (int i = 0; i < m_hand.Count; i++) {
                Vector3 v3 = m_hand[i].GetGameObject().transform.localPosition;
                v3.y = i - (m_hand.Count / 2);
                m_hand[i].GetGameObject().transform.localPosition = v3;
                Debug.Log("RepositionCardsInHand: y: " + v3.y);
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