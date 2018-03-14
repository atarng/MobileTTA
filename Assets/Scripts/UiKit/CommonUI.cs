///////////////////////////////
// TODO: Figure out wtf this class is for.
// 
///////////////////////////////

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class CommonUI : MonoBehaviour {
    [SerializeField]
    Text m_coinsText;
    [SerializeField]
    Text m_salvageText;

    public void UpdateCoinText(string s) {
        m_coinsText.text = s;
    }
    public void UpdateSalvageText(string s) {
        m_salvageText.text = s;
    }
}
