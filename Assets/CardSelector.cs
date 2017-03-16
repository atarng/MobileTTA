using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;

public class CardSelector : MonoBehaviour {
    DeckManager m_dmRef = null;
    int m_unit_def_id = -1;
    int m_count = 0;

    // tier
    //bool pressed = false;
    [SerializeField]
    private Transform m_artAttachmentPoint;

    public void SetInfo(DeckManager dm, int id, int count) {
        m_dmRef = dm;

        m_unit_def_id = id;
        m_count = count;

        UnitManager um = SingletonMB.GetInstance<UnitManager>();
        UnitManager.UnitDefinition ud = um.GetDefinition(m_unit_def_id);
        GameObject go = GameObject.Instantiate(um.GetArtFromKey(ud.ArtKey));
        go.transform.SetParent(m_artAttachmentPoint);
    }


    //private void Update() {
    private void OnMouseOver() {
        //Debug.Log("[CardSelector/OnMouseOver]");
        if (Input.GetMouseButtonDown(0)) {
            // Add Unit
            m_dmRef.AddUnit(m_unit_def_id);
        }
        if (Input.GetMouseButtonDown(1)) {
            // Remove Unit

        }
    }

    /*
    private void OnMouseDown() {

    }
    private void OnMouseUp() {

    }
    */
}