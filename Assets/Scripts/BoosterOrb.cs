using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using AtRng.MobileTTA;

public class BoosterOrb : MonoBehaviour {
    BoosterMB m_ref;
    public void SetReference(BoosterMB bmb) {
        m_ref = bmb;
    }

    public void GetRandomUnitAndDisplayIt() {
        UnitManager.UnitDefinition ud = m_ref.GetRandomUnit();

        Debug.Log("Open An Orb: " + ud.DefinitionID);

        m_ref.SaveUnitToCollection(ud);

        GameObject artInstance = GameObject.Instantiate(SingletonMB.GetInstance<UnitManager>().GetArtFromKey(ud.ArtKey)).gameObject;
        artInstance.transform.position = transform.position;

        artInstance.transform.SetParent(transform.parent);
        artInstance.transform.localScale = Vector3.one;

        m_ref.AddArt(artInstance, this);
    }
}