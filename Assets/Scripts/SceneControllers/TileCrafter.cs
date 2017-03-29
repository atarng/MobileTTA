using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;
using System;

public class TileCrafter : SceneControl, ICardSelectorHandler {
    [SerializeField]
    RectTransform m_commonUI;
    CommonUI m_commonUIInstance;

    [SerializeField]
    GridLayoutGroup m_gridLayoutGroup;

    [SerializeField]
    CardSelector m_cardSelectorPrefab;
    List<CardSelector> m_cardSelectors = new List<CardSelector>();

    List<UnitManager.UnitDefinition> m_allUnits;
    HashSet<int> m_ownedUnits = new HashSet<int>();
    private void Start() {
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

        m_allUnits = UnitManager.GetInstance<UnitManager>().GetAsCollection();

        List<UnitManager.UnitPersistence> collectionList = SaveGameManager.GetSaveGameData().LoadFrom("Collection") as List<UnitManager.UnitPersistence>;
        for (int i = 0; i < collectionList.Count; i++) {
            if (!m_ownedUnits.Contains(collectionList[i].DefinitionID)) {
                m_ownedUnits.Add(collectionList[i].DefinitionID);
            }
        }

        InstantiateCollectionObjects();
    }

    private void InstantiateCollectionObjects() {
        for (int i = 0; i < m_allUnits.Count; i++) {
            CardSelector cs = GameObject.Instantiate<CardSelector>(m_cardSelectorPrefab);

            cs.SetCardSelectorHandler(this);
            cs.SetInfo(m_allUnits[i].DefinitionID, System.Guid.Empty);
            cs.transform.SetParent(m_gridLayoutGroup.transform);
            cs.transform.localScale = Vector3.one;

            cs.SetOverlay(!m_ownedUnits.Contains(m_allUnits[i].DefinitionID));

            m_cardSelectors.Add(cs);
        }
    }

    public void HandleClick(CardSelector cs) {
        CraftTile(cs);
    }

    public void CraftTile( CardSelector cs ){
        List<UnitManager.UnitPersistence> collection = SaveGameManager.GetSaveGameData().LoadFrom("Collection") as List<UnitManager.UnitPersistence>;

        UnitManager.UnitPersistence newBoosterUnit = new UnitManager.UnitPersistence(cs.DefinitionID);
        collection.Add(newBoosterUnit);

        SaveGameManager.GetSaveGameData().SaveTo("Collection", collection);
        SaveGameManager.Save();

        //
        cs.SetOverlay(false);
    }
}
