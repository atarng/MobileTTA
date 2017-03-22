using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {
    // ScriptableObject
    public class LevelScriptableObject : ScriptableObject {
        [SerializeField]
        int[] m_opposingDeckList;

        [SerializeField]
        LevelEditorPlaceable[] m_placeablesArray;
        public LevelEditorPlaceable[] PlaceablesArray {
            get {
                return m_placeablesArray;
            }
        }
        [SerializeField]
        int m_width, m_height;
        public int Width {
            get {
                return m_width;
            }
        }
        public int Height {
            get {
                return m_height;
            }
        }

        public void ReadLevelEditorGrid(LevelEditorGrid toPullFrom) {
            LevelEditorPlaceable[] toCopy = toPullFrom.GetPlaceablesList();
            m_placeablesArray = new LevelEditorPlaceable[toCopy.Length];
            for (int i = 0; i < m_placeablesArray.Length; i++) {
                m_placeablesArray[i] = toCopy[i];
            }
            m_width = toPullFrom.Width;
            m_height = toPullFrom.Height;
        }
    }
}