using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AtRng.MobileTTA;

public class CardSelector : MonoBehaviour {
    DeckManager m_dmRef = null;

    int m_unit_def_id = -1;
    bool m_selected = false;
    public bool Selected {
        get { return m_selected; }
        set {
            m_selected = value;
            transform.localScale = m_selected ? (Vector3.one * 1.2f) : Vector3.one;
        }
    }

    System.Guid m_unitGuid;

    Vector3 m_mouseDownPosition = Vector3.zero;
    //float m_clickTimeout = 0;

    //  int m_count = 0;
    //  int tier


    //bool pressed = false;
    [SerializeField]
    private Transform m_artAttachmentPoint;

    public void SetInfo(DeckManager dm, int id, System.Guid unitGuid) {
        m_dmRef = dm;

        m_unit_def_id = id;
        m_unitGuid = unitGuid;

        //m_count = count;

        UnitManager um = SingletonMB.GetInstance<UnitManager>();
        UnitManager.UnitDefinition ud = um.GetDefinition(m_unit_def_id);
        GameObject go = GameObject.Instantiate(um.GetArtFromKey(ud.ArtKey));
        go.transform.SetParent(m_artAttachmentPoint);
    }

    //private void Update() {
    private void OnMouseOver() {
        //Debug.Log("[CardSelector/OnMouseOver]");
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            // Add Unit
            m_mouseDownPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) {
            // Add Unit
            Vector3 mousePos = Input.mousePosition;
            if (Vector3.Distance(m_mouseDownPosition, mousePos) < 10) {
                if (m_selected) {
                    m_dmRef.RemoveUnit(m_unitGuid);
                    Selected = false;
                }
                else if (m_dmRef.AddUnit(m_unitGuid)) { 
                    Selected = true;
                }
            }
        }
/*
        if (Input.GetMouseButtonUp(0)) {
            m_dmRef.AddUnit(m_unitGuid);
        }
        if (Input.GetMouseButtonUp(1)) {
            // Remove Unit
            m_dmRef.RemoveUnit(m_unitGuid);
        }
//*/
    }

}