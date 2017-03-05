using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AtRng.MobileTTA {
    public class SingletonMB : MonoBehaviour {
        public static SingletonMB Instance;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }

        public static T GetInstance<T>() {
            return (T) Convert.ChangeType(Instance, typeof(T));
        }
    }
}