﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;


namespace AtRng.MobileTTA {
    public class SingletonMB : MonoBehaviour {
        //public static SingletonMB Instance;

        //public static List<SingletonMB> ListOfSingletons = new List<SingletonMB>();
        public static Dictionary<string, SingletonMB> SingletonMap = new Dictionary<string, SingletonMB>();

        private void Awake() {
            if (SingletonMap.ContainsKey(this.GetType().Name)) {
                Destroy(gameObject);
            }
            else {
                SingletonMap.Add(this.GetType().Name, this);
                OnAwake();
            }
/*
            if (Instance == null) {
                Instance = this;
                //ListOfSingletons.Add(this);
                //SingletonMap.Add(this.GetType().Name, this);
            }
            else {
                SingletonMap.Add(this.GetType().Name, this);
            }
*/
        }
        protected virtual void OnAwake() {}
        
        public static T GetInstance<T>() where T : SingletonMB {
            return (T) Convert.ChangeType(SingletonMap[typeof(T).Name], typeof(T));
        }

/*
        public static T GetRegisteredInstance<T>() where T : SingletonMB {
            //return SingletonMap[T.ToString()];
            for (int i = 0; i < ListOfSingletons.Count; i++) {
                if (ListOfSingletons[i] is T) {
                    T ret = ListOfSingletons[i] as T;
                    return ret;
                }
            }
            return null;
        }
//*/
    }
}