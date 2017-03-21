using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;

public abstract class SceneControl : SingletonMB {
    static SceneControl s_currentSceneControl;

    [SerializeField]
    Text m_textRef;
    float textTime = 0;
    Color m_zeroColor = new Color(0, 0, 0, 0);

    public void DisplayInfo(string info) {
        if (m_textRef != null) {
            m_textRef.text = info;
            Color toAssign = Color.white;
            toAssign.a = 1;
            m_textRef.color = toAssign;
            
            Debug.Log(info);
        }
        textTime = 2;
    }
    public void DisplayWarning(string info) {
        if (m_textRef != null) {
            m_textRef.text = info;

            Color toAssign = Color.yellow;
            toAssign.a = 1;
            m_textRef.color = toAssign;

            Debug.LogWarning(info);
        }
        textTime = 2;
    }
    public void DisplayError(string info) {
        if (m_textRef != null) {
            m_textRef.text = info;

            Color toAssign = Color.red;
            toAssign.a = 1;
            m_textRef.color = toAssign;

            Debug.LogError(info);
        }
        textTime = 2;
    }

    protected override void OnAwake() {
        s_currentSceneControl = this;
    }

    protected override void OnDestroy() {
        if(s_currentSceneControl == this) {
            s_currentSceneControl = null;
        }

       base.OnDestroy();
    }

    protected virtual void Update() {
        if (m_textRef) {
            if(textTime > -1) {
                Color textColor = m_textRef.color;
                textColor.a = Mathf.Lerp(0, textColor.a, Mathf.Clamp(1 + textTime,0,1));
                m_textRef.color = textColor;
                textTime -= Time.deltaTime;
            }
            else {
                m_textRef.color = m_zeroColor;
                textTime = -1;
            }
        }
    }

    public static SceneControl GetCurrentSceneControl() {
        return s_currentSceneControl;
    }
}
