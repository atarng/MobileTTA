using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AtRng.MobileTTA {
    public enum PlaceableType {
        Unit,
        Impassable
    }

    [Serializable]
    public struct LevelEditorPlaceable {
        public PlaceableType placeableType;
        public int X;
        public int Y;
        public int ID;
        public int PlayerID;
    }

    // Monobehavior
    public class LevelEditorGrid : Grid {
        [SerializeField]
        LevelEditorPlaceable[] placeablesArray;
        public LevelEditorPlaceable[] GetPlaceablesList() {
            return placeablesArray;
        }

        public LevelScriptableObject AsScriptableObject() {
            //new LevelScriptableObject(placeablesArray);

            LevelScriptableObject ret = ScriptableObject.CreateInstance<LevelScriptableObject>();
            ret.ReadLevelEditorGrid(this);
            return ret;
        }
        public void ReadScriptableObject(LevelScriptableObject lso) {
            placeablesArray = new LevelEditorPlaceable[lso.PlaceablesArray.Length];
            for (int i = 0; i < placeablesArray.Length; i++) {
                placeablesArray[i] = lso.PlaceablesArray[i];
            }
            Width  = lso.Width;
            Height = lso.Height;
        }
    }
}