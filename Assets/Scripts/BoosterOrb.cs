using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using AtRng.MobileTTA;

public class BoosterOrb : MonoBehaviour {
    BoosterMB m_ref;

    [SerializeField]
    private Transform m_artPlacement;
    [SerializeField]
    private GameObject m_villageArt;

    public void SetReference(BoosterMB bmb) {
        m_ref = bmb;
    }

    public void GetRandomUnitAndDisplayIt() {
        UnitManager.UnitDefinition ud = m_ref.GetRandomUnit();

        Debug.Log("Open An Orb: " + ud.DefinitionID);

        m_ref.SaveUnitToCollection(ud);

        ArtPrefab ap = SingletonMB.GetInstance<UnitManager>().GetArtFromKey(ud.ArtKey);
        
        GameObject artInstance = 
            GameObject.Instantiate(ap).gameObject;
        
        artInstance.transform.SetParent(m_artPlacement); //transform.parent);

        artInstance.transform.position   = transform.position;
        artInstance.transform.localScale = Vector3.one;
        //artInstance.transform.localPosition = ap.transform.localPosition;
        //artInstance.transform.localScale    = ap.transform.localScale;

        Destroy(m_villageArt);

        //m_ref.AddArt(artInstance, this);
    }
}