using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;

public class ActionPoint_MB : MonoBehaviour {

    public enum AP_STATE {
        IDLE = 0,
        SPENT = 1,
        PENDING = 2,
    }

    [SerializeField]
    Image m_image; //SpriteRenderer m_sr;
    

    bool m_available = false;

    BasePlayer m_bp = null;
    GameObject m_listenForMouseUp = null;

    Animator m_animator;

    private AP_STATE m_state;

    private const string anim_state = "anim_state";

    #region MONOBEHAVIORS
    private void Start()
    {
        m_animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (m_listenForMouseUp != null)
        {
            Vector3 v3 = CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition);
            v3.z = 0;
            m_listenForMouseUp.transform.position = v3;
        }
    }
    private void OnMouseDown()
    {
        //m_sr.color = Color.grey;
        if (m_available && m_bp.GetEnoughActionPoints(1))
        {
            //m_listenForMouseUp = true;
            m_listenForMouseUp = GameObject.Instantiate(gameObject);
            m_listenForMouseUp.transform.SetParent(transform);
            m_listenForMouseUp.transform.localScale = Vector3.one;

            // appear used up.
            SetState(AP_STATE.SPENT);//SetAppearance(false);
        }
    }
    private void OnMouseUp()
    {
        if (m_listenForMouseUp)
        {
            Vector3 v3 = CameraManager.Instance.GameCamera().ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D rh2d = Physics2D.Raycast(new Vector2(v3.x, v3.y), Vector2.zero, 0f, 1 << LayerMask.NameToLayer("Units"));
            bool applied = false;
            if (rh2d)
            {
                BaseUnit bu = rh2d.transform.GetComponent<BaseUnit>();
                if (bu != null)
                {
                    Debug.Log("Apply Enhancement to bu: " + bu.name);
                    applied = true;
                }
            }
            if (!applied)
            {
                // restore appearance.
                //m_sr.color = Color.blue;
                //SetAppearance(true);
                SetState(AP_STATE.IDLE);
            }
            else
            {
                m_bp.ExpendUnitActionPoint();
            }
            Destroy(m_listenForMouseUp);
        }
    }
    #endregion

    public void SetState(AP_STATE state)
    {
        if(m_animator == null) {
            m_animator = GetComponent<Animator>();
        }

        if (m_state != state) {
            m_state = state;
            switch (state) {
                default:
                    m_animator.SetInteger(anim_state, (int)state);
                    break;
            }
        }
    }

    public void SetAppearance(bool on) {
        m_image.color = (on) ? Color.blue : Color.grey;
        m_available = on;
    }

    public void AssignPlayerReference(BasePlayer bp) {
        m_bp = bp;
    }

}