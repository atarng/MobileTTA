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

    private void Start() {
        InstantiateOrbs();
        m_allUnits = UnitManager.GetInstance<UnitManager>().GetAsCollection();
    }

    public void InstantiateOrbs() {

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

        int randomIndex = UnityEngine.Random.Range(1, m_allUnits.Count);
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