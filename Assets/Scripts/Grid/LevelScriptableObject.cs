using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {
    // ScriptableObject
    public class LevelScriptableObject : ScriptableObject {
        [SerializeField]
        int m_width, m_height;
        public int Width { get { return m_width; } }
        public int Height { get { return m_height; } }

        [SerializeField]
        bool m_useAIOpponent = false;
        public bool UsesAIOpponent { get { return m_useAIOpponent; } }

        [SerializeField]
        bool m_usesPlayerDeckList = true;
        public bool UsesPlayerDeckList { get { return m_usesPlayerDeckList; } }

        [SerializeField]
        bool m_usesOpponentDeck = true;
        public bool UsesOpponentDeckList { get { return m_usesOpponentDeck; } }

        [SerializeField]
        UnitManager.UnitPersistence[] m_opposingDeckList;
        public UnitManager.UnitPersistence[] OpposingDeckList {
            get {
                return m_opposingDeckList;
            }
        }

        [SerializeField]
        LevelEditorPlaceable[] m_placeablesArray;
        public LevelEditorPlaceable[] PlaceablesArray {
            get {
                return m_placeablesArray;
            }
        }

        [SerializeField]
        string tutorial_prefab_name;

#if UNITY_EDITOR
        public void ReadLevelEditorGrid(LevelEditorGrid toPullFrom) {
            LevelEditorPlaceable[] toCopy = toPullFrom.GetPlaceablesList();
            m_placeablesArray = new LevelEditorPlaceable[toCopy.Length];
            for (int i = 0; i < m_placeablesArray.Length; i++) {
                m_placeablesArray[i] = toCopy[i];
            }

            UnitManager.UnitPersistence[] copyDeck = toPullFrom.GetOpponentDeckList();
            m_opposingDeckList = new UnitManager.UnitPersistence[copyDeck.Length];
            for (int i = 0; i < m_opposingDeckList.Length; i++) {
                m_opposingDeckList[i] = copyDeck[i];
            }

            m_width = toPullFrom.Width;
            m_height = toPullFrom.Height;

            m_usesPlayerDeckList = toPullFrom.UsesPlayerDeckList();
            m_useAIOpponent = toPullFrom.UsesAIOpponent();
        }
#endif

    }
}