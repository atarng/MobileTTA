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

        [SerializeField]
        UnitManager.UnitPersistence[] m_opponentDeckList;
        public UnitManager.UnitPersistence[] GetOpponentDeckList() {
            return m_opponentDeckList;
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

            m_opponentDeckList = new UnitManager.UnitPersistence[lso.OpposingDeckList.Length];
            for (int i = 0; i < m_opponentDeckList.Length; i++) {
                m_opponentDeckList[i] = lso.OpposingDeckList[i];
            }

            Width  = lso.Width;
            Height = lso.Height;
        }
    }
}