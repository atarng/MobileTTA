using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {

    public class Player : MonoBehaviour, IPlaceable, IGamePlayer {

        int m_health = 0;

        int IGamePlayer.GetCurrentDrawCost() {
            throw new NotImplementedException();
        }

        List<IUnit> IGamePlayer.GetCurrentSummonedUnits() {
            throw new NotImplementedException();
        }

        List<IUnit> IGamePlayer.GetHand() {
            throw new NotImplementedException();
        }

        // Drawing costs, etc.
        void IGamePlayer.Reset() {
            throw new NotImplementedException();
        }
        int IGamePlayer.GetHealth() {
            return m_health;
        }

        public bool IsDragging() {
            throw new NotImplementedException();
        }

        public void SetDragging() {
            throw new NotImplementedException();
        }

        public bool AttemptRelease(Tile sourceTile, Tile destinationTile) {
            throw new NotImplementedException();
        }

        public GameObject GetGameObject() {
            throw new NotImplementedException();
        }
    }
}