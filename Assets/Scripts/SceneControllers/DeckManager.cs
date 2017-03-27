using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;

public class DeckManager : SceneControl {
    [SerializeField]
    CardSelector m_cardSelector;

    /*** POSITION / UI TRANSFORMS ***/
    [SerializeField]
    GridLayoutGroup m_gridLayoutGroup;
    [SerializeField]
    HorizontalLayoutGroup m_horizontalLayoutGroup;



    const int MAX_DECK_SIZE = 10;

    [SerializeField]
    RectTransform m_commonUI;
    CommonUI m_commonUIInstance;

    List<UnitManager.UnitPersistence> m_deckList = null;
    Dictionary<System.Guid, UnitManager.UnitPersistence> m_Collection = null;
    //
    List<CardSelector> m_cardSelectors = new List<CardSelector>();

    /*** UI ELEMENTS ***/
    [SerializeField]
    Text m_deckCountText;
    [SerializeField]
    Text m_collectionCountText;
    [SerializeField]
    Image m_examineImage;
    [SerializeField]
    Text m_attackValueText;
    //[SerializeField] // might just be an icon
    //Text m_attackTypeText;
    [SerializeField]
    Text m_movementText;
    [SerializeField]
    Text m_physicalHealthText;
    [SerializeField]
    Text m_spiritualHealthText;

    /*** DEBUG / TEMP ***/
    [SerializeField]
    bool m_debug = false;
    [SerializeField]
    int[] DebugList; // Collection Equivalent.

    // Use this for initialization
    void Start () {
        SaveGameManager.Load();

        // COMMON UI
        RectTransform go = GameObject.Instantiate(m_commonUI);
        go.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.localScale = Vector3.one;
        go.offsetMin = Vector2.zero;
        go.offsetMax = Vector2.zero;

        m_commonUIInstance = go.GetComponent<CommonUI>();
        if (m_commonUIInstance != null) {
            m_commonUIInstance.UpdateCoinText(SingletonMB.GetInstance<AccountManager>().GetCoins().ToString());
            m_commonUIInstance.UpdateSalvageText(SingletonMB.GetInstance<AccountManager>().GetSalvage().ToString());
        }

        if (SaveGameManager.GetSaveGameData().Exists("TestDeck")) {
            m_deckList = SaveGameManager.GetSaveGameData().LoadFrom("TestDeck") as List<UnitManager.UnitPersistence>;
            Debug.Log("[DeckManager] DeckSizeCount: " + m_deckList.Count);
        }
        else {
            m_deckList = new List<UnitManager.UnitPersistence>();
        }

        if (m_debug) {
            for (int i = 0; i < DebugList.Length; i++) {
                CardSelector cs = GameObject.Instantiate<CardSelector>(m_cardSelector);
                cs.SetInfo(this, DebugList[i], System.Guid.Empty);
                cs.transform.SetParent(transform);

                Vector3 newPosition = Vector3.zero;
                newPosition.x = i * 100;
                cs.transform.localPosition = newPosition;
            }
        }
        else {
            bool collection_Handled = false;
            if (SaveGameManager.GetSaveGameData().Exists("Collection")) {
                List<UnitManager.UnitPersistence> collectionList = SaveGameManager.GetSaveGameData().LoadFrom("Collection") as List<UnitManager.UnitPersistence>;
                m_Collection = new Dictionary<System.Guid, UnitManager.UnitPersistence>();
                if (collectionList != null) {
                    for (int i = 0; i < collectionList.Count; i++) {
                        m_Collection.Add(collectionList[i].UnitID, collectionList[i]);
                    }
                    collection_Handled = true;
                }
            }

            if(!collection_Handled){
                // Initialize a Basic Collection
                InitializeCollection();
            }
            // 
            InstantiateCollectionObjects();
        }

        m_collectionCountText.text = string.Format("{0}", m_Collection.Count);
        m_deckCountText.text = string.Format("{0}/{1}", m_deckList.Count, MAX_DECK_SIZE);

    }

    public Transform GetDeckTransform() {
        return m_horizontalLayoutGroup.transform;
    }

    public Transform GetCollectionTransform() {
        return m_gridLayoutGroup.transform;
    }

    public void PreviewUnit(System.Guid unitGuid) {
        if (m_Collection.ContainsKey(unitGuid)) {
            UnitManager um = SingletonMB.GetInstance<UnitManager>();
            UnitManager.UnitPersistence up = m_Collection[unitGuid];
            UnitManager.UnitDefinition ud  = um.GetDefinition( up.DefinitionID );
            if (ud != null) {
                m_attackValueText.text     = ud.AttackValue.ToString();
                m_physicalHealthText.text  = ud.PhysicalHealth.ToString();
                m_spiritualHealthText.text = ud.SpiritualHealth.ToString();
                m_movementText.text        = ud.Movement.ToString();

                m_examineImage.sprite = um.GetArtFromKey(ud.ArtKey).ImageRef.sprite;
            }
        }
    }

    public bool AddUnit( System.Guid unitGuid ) { //defId ) {
        bool unitAdded = false;
        if (m_deckList.Count < MAX_DECK_SIZE) {
            //UnitManager.GetInstance<UnitManager>().GetDefinition(defId)
            if (m_Collection.ContainsKey(unitGuid)) {
                UnitManager.UnitPersistence up = m_Collection[unitGuid];

                //Debug.Log("[DeckManager/AddUnit] AddUnit DefId: " + up.DefinitionID);

                m_deckList.Add( up );
                unitAdded = true;
            }
            else {
                Debug.LogError("This should not be possible");
            }
        }
        else {
            //Debug.LogWarning("[DeckManager/AddUnit] Max Deck Size Reached.");
            SceneControl.GetCurrentSceneControl().DisplayWarning("Max Deck Size Reached.");
        }

        m_deckCountText.text = string.Format("{0}/{1}", m_deckList.Count, MAX_DECK_SIZE);

        return unitAdded;
    }

    public void RemoveUnit( System.Guid unitGuid ) {
        if (m_Collection.ContainsKey(unitGuid)) {

            //Debug.Log("[DeckManager/AddUnit] Remove DefId: " + up.DefinitionID);

            // probably want to verify that this object actually exists in the deck.
            m_deckList.Remove(m_Collection[unitGuid]);
        }

        m_deckCountText.text = string.Format("{0}/{1}", m_deckList.Count, MAX_DECK_SIZE);

    }


    public void ClearCollection() {
        ClearDeck();

        for (int i = 0; i < m_cardSelectors.Count; i++) {
            Destroy(m_cardSelectors[i].gameObject);
        }
        m_cardSelectors.Clear();

        InitializeCollection();
        InstantiateCollectionObjects();

        m_collectionCountText.text = string.Format("{0}", m_Collection.Count);

    }

    public void ClearDeck() {
        if (m_deckList != null) {
            m_deckList.Clear();
        }
        for (int i = 0; i < m_cardSelectors.Count; i++) {
            m_cardSelectors[i].Selected = false;
        }

        m_deckCountText.text = string.Format("{0}/{1}", m_deckList.Count, MAX_DECK_SIZE);

    }
    public void SaveDeck() {
        Debug.Log("[DeckManager/SaveDeck]");
        SaveGameManager.GetSaveGameData().SaveTo("TestDeck", m_deckList);
        SaveGameManager.Save();
    }


    private void InitializeCollection() {
        m_Collection = new Dictionary<System.Guid, UnitManager.UnitPersistence>();

        //UnitManager.GetInstance<UnitManager>().GetDefinition(1)
        UnitManager.UnitPersistence up1 = new UnitManager.UnitPersistence(1, false);
        UnitManager.UnitPersistence up2 = new UnitManager.UnitPersistence(2, false);
        UnitManager.UnitPersistence up3 = new UnitManager.UnitPersistence(3, false);

        m_Collection.Add(up1.UnitID, up1);
        m_Collection.Add(up2.UnitID, up2);
        m_Collection.Add(up3.UnitID, up3);

        SaveGameManager.GetSaveGameData().SaveTo("Collection", m_Collection.Values.ToList());
        SaveGameManager.Save();
    }

    private void InstantiateCollectionObjects() {
        for (Dictionary<System.Guid, UnitManager.UnitPersistence>.Enumerator iter = m_Collection.GetEnumerator(); iter.MoveNext();) {
            CardSelector cs = GameObject.Instantiate<CardSelector>(m_cardSelector);

            cs.SetInfo(this, iter.Current.Value.DefinitionID, iter.Current.Value.UnitID);

            //cs.transform.SetParent(m_gridLayoutGroup.transform);
            cs.Selected = (m_deckList.Find(x => x.UnitID == iter.Current.Value.UnitID) != null);

            m_cardSelectors.Add(cs);
        }
    }

}
