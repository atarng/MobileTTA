using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;

public class BoosterMB : MonoBehaviour {
    [SerializeField]
    BoosterOrb booster_orb_prefab;

    List<UnitManager.UnitDefinition> m_allUnits;

    private void Start() {
        for (int i = 0; i < 5; i++) {
            BoosterOrb go = GameObject.Instantiate<BoosterOrb>(booster_orb_prefab);
            go.transform.SetParent(transform);
            go.SetReference(this);

            Vector3 toPos = Vector3.zero;

            toPos.x = 300 * Mathf.Cos( (i * (2 * Mathf.PI) / 5) + (Mathf.PI * 0.5f));
            toPos.y = 300 * Mathf.Sin( (i * (2 * Mathf.PI) / 5) + (Mathf.PI * 0.5f));

            go.transform.localPosition = toPos;
            go.transform.localScale = Vector3.one;

            /*
            Button b = go.GetComponent<Button>();
            if (b != null) {
                b.onClick.AddListener(GetRandomUnit);
            }
            */
        }

        m_allUnits = UnitManager.GetInstance<UnitManager>().GetAsCollection();
    }

    
    public UnitManager.UnitDefinition GetRandomUnit() {
        if (m_allUnits == null || m_allUnits.Count == 0) {
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, m_allUnits.Count);
        return m_allUnits[randomIndex];
    }

    public void SaveUnitToCollection( UnitManager.UnitDefinition ud ) {
        List<UnitManager.UnitDefinition> collection = SaveGameManager.GetSaveGameData().LoadFrom("Collection") as List<UnitManager.UnitDefinition>;
        collection.Add(ud);
        SaveGameManager.GetSaveGameData().SaveTo("Collection", collection);
        SaveGameManager.Save();
    }
}