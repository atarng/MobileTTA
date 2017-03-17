using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;

public class DeckManager : MonoBehaviour {
    [SerializeField]
    CardSelector m_cardSelector;

    [SerializeField]
    bool m_debug = false;
    [SerializeField]
    int[] DebugList; // Collection Equivalent.

    const int MAX_DECK_SIZE = 10;

    List<UnitManager.UnitDefinition> m_deckList = null;
    List<UnitManager.UnitDefinition> m_Collection = null;
    // Use this for initialization
    void Start () {
        SaveGameManager.Load();

        if (SaveGameManager.GetSaveGameData().Exists("TestDeck")) {
            m_deckList = SaveGameManager.GetSaveGameData().LoadFrom("TestDeck") as List<UnitManager.UnitDefinition>;
            Debug.Log("[DeckManager] DeckSizeCount: " + m_deckList.Count);
        }
        else {
            m_deckList = new List<UnitManager.UnitDefinition>();
        }

        if (m_debug) {
            for (int i = 0; i < DebugList.Length; i++) {
                CardSelector cs = GameObject.Instantiate<CardSelector>(m_cardSelector);
                cs.SetInfo(this, DebugList[i], 0);
                cs.transform.SetParent(transform);

                Vector3 newPosition = Vector3.zero;
                newPosition.x = i * 100;
                cs.transform.localPosition = newPosition;
            }
        }
        else {
            if (SaveGameManager.GetSaveGameData().Exists("Collection")) {
                m_Collection = SaveGameManager.GetSaveGameData().LoadFrom("Collection") as List<UnitManager.UnitDefinition>;
            }
            else {
                // Initialize a Basic Collection
                InitializeCollection();
            }

            for (int i = 0; i < m_Collection.Count; i++) {
                CardSelector cs = GameObject.Instantiate<CardSelector>(m_cardSelector);
                cs.SetInfo(this, m_Collection[i].ID, 0);
                cs.transform.SetParent(transform);

                Vector3 newPosition = Vector3.zero;
                newPosition.x = i * 100;
                cs.transform.localPosition = newPosition;
            }
        }

    }


    public void AddUnit( int defId ) {
        Debug.Log("[DeckManager/AddUnit] AddUnit DefId: " + defId);
        if (m_deckList.Count < MAX_DECK_SIZE) {
            m_deckList.Add(UnitManager.GetInstance<UnitManager>().GetDefinition(defId));
        }
        else {
            Debug.LogWarning("[DeckManager/AddUnit] Max Deck Size Reached.");
        }
    }

    public void ClearDeck() {
        Debug.Log("[DeckManager/ClearDeck]");

        InitializeCollection();

        if (m_deckList != null) {
            m_deckList.Clear();
        }
    }
    public void SaveDeck() {
        Debug.Log("[DeckManager/SaveDeck]");
        SaveGameManager.GetSaveGameData().SaveTo("TestDeck", m_deckList);
        SaveGameManager.Save();
    }


    private void InitializeCollection() {
        m_Collection = new List<UnitManager.UnitDefinition>();
        m_Collection.Add(UnitManager.GetInstance<UnitManager>().GetDefinition(1));
        m_Collection.Add(UnitManager.GetInstance<UnitManager>().GetDefinition(2));
        m_Collection.Add(UnitManager.GetInstance<UnitManager>().GetDefinition(3));

        SaveGameManager.GetSaveGameData().SaveTo("Collection", m_Collection);
        SaveGameManager.Save();
    }

}
