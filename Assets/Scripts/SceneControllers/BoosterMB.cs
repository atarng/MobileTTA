using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;

public class BoosterMB : SceneControl {
    [SerializeField]
    BoosterOrb booster_orb_prefab;

    List<BoosterOrb> boosterOrbList = new List<BoosterOrb>();
    List<GameObject> artList = new List<GameObject>();

    List<UnitManager.UnitDefinition> m_allUnits;

    [SerializeField]
    RectTransform m_commonUIPrefab;
    CommonUI m_commonUIInstance;

    private void Start() {
        //InstantiateOrbs();
        m_allUnits = UnitManager.GetInstance<UnitManager>().GetAsCollection();

        RectTransform go = GameObject.Instantiate(m_commonUIPrefab);
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
    }

    public void InstantiateOrbs() {

        if (SingletonMB.GetInstance<AccountManager>().GetCoins() < 5) {
            SceneControl.GetCurrentSceneControl().DisplayWarning("Not Enough Coins to Open Pack.");
            //
            SingletonMB.GetInstance<AccountManager>().ModifyCoins(10);
            m_commonUIInstance.UpdateCoinText(SingletonMB.GetInstance<AccountManager>().GetCoins().ToString());
            m_commonUIInstance.UpdateSalvageText(SingletonMB.GetInstance<AccountManager>().GetSalvage().ToString());
            //
            return;
        }
        else {
            SingletonMB.GetInstance<AccountManager>().ModifyCoins(-5);
            m_commonUIInstance.UpdateCoinText(SingletonMB.GetInstance<AccountManager>().GetCoins().ToString());
        }

        while (boosterOrbList.Count > 0) {
            if(boosterOrbList[0] != null) {
                Destroy(boosterOrbList[0].gameObject);
            }
            boosterOrbList.RemoveAt(0);
        }

        while (artList.Count > 0) {
            if (artList[0] != null) {
                Destroy(artList[0].gameObject);
            }
            artList.RemoveAt(0);
        }

        for (int i = 0; i < 5; i++) {
            BoosterOrb go = GameObject.Instantiate<BoosterOrb>(booster_orb_prefab);
            go.transform.SetParent(transform);
            go.SetReference(this);

            Vector3 toPos = Vector3.zero;

            toPos.x = 300 * Mathf.Cos((i * (2 * Mathf.PI) / 5) + (Mathf.PI * 0.5f));
            toPos.y = 300 * Mathf.Sin((i * (2 * Mathf.PI) / 5) + (Mathf.PI * 0.5f));

            go.transform.localPosition = toPos;
            go.transform.localScale = Vector3.one;

            boosterOrbList.Add(go);
        }
    }

    public void AddArt(GameObject art){
        artList.Add(art);
    }

    public UnitManager.UnitDefinition GetRandomUnit() {
        if (m_allUnits == null || m_allUnits.Count == 0) {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, m_allUnits.Count);
        return m_allUnits[randomIndex];
    }

    public void SaveUnitToCollection( UnitManager.UnitDefinition ud ) {
        List<UnitManager.UnitPersistence> collection = SaveGameManager.GetSaveGameData().LoadFrom("Collection") as List<UnitManager.UnitPersistence>;

        UnitManager.UnitPersistence newBoosterUnit = new UnitManager.UnitPersistence(ud.DefinitionID);
        collection.Add(newBoosterUnit);

        SaveGameManager.GetSaveGameData().SaveTo("Collection", collection);
        SaveGameManager.Save();
    }
}