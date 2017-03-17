using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterOrb : MonoBehaviour {
    BoosterMB m_ref;
    public void SetReference(BoosterMB bmb) {
        m_ref = bmb;
    }

    public void GetRandomUnitAndDisplayIt() {
        UnitManager.UnitDefinition ud = m_ref.GetRandomUnit();

        Debug.Log("Open An Orb: " + ud.ID);

        m_ref.SaveUnitToCollection(ud);
        Destroy(gameObject);
    }
}