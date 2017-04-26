using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;

/***
 * CardSelector is the class that handles the card/unit behavior while on the deckbuilding
 * or tile crafting scenes.
 */
public class CardSelector : MonoBehaviour {
    bool m_selected = false;
    public bool Selected {
        get { return m_selected; }
        set {
            m_selected = value;
        }
    }

    int m_unit_def_id = -1;
    public int DefinitionID {
        get { return m_unit_def_id; }
    }
    System.Guid m_unitGuid;
    public System.Guid UnitID {
        get { return m_unitGuid; }
    }

    //bool pressed = false;
    [SerializeField]
    private Transform m_artAttachmentPoint;

    [SerializeField]
    private Image m_overlay;
    bool overlayOn = false;

    ICardSelectorHandler m_icsh = null;
    public void SetCardSelectorHandler(ICardSelectorHandler icsh) {
        m_icsh = icsh;
    }

    public void SetInfo(int id, System.Guid unitGuid) {
        m_unit_def_id = id;
        m_unitGuid = unitGuid;

        UnitManager um = SingletonMB.GetInstance<UnitManager>();
        UnitManager.UnitDefinition ud = um.GetDefinition(m_unit_def_id);
        ArtPrefab go = GameObject.Instantiate<ArtPrefab>(um.GetArtFromKey(ud.ArtKey));
        go.transform.SetParent(m_artAttachmentPoint);

        go.transform.localPosition = Vector3.zero;
        go.transform.localScale    = Vector3.one;
    }

    public void SetOverlay(bool on) {
        m_overlay.color = on ? TileColors.GREY : TileColors.CLEAR;
    }

    public void PreviewOrToggleUnit() {
        m_icsh.HandleClick(this);
    }
}