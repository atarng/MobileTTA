using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AtRng.MobileTTA;

public class TextLoader : MonoBehaviour {

	[SerializeField]
	private Text m_text_field;

	[SerializeField]
	private string m_localization_key;

	// Use this for initialization
	void Start () {
		LoadLocalizationKeyAsText();		
	}
	
	void LoadLocalizationKeyAsText() {
		string t = SingletonMB.GetInstance<LocalizationManager>().
				GetTextFromKey(m_localization_key);
		m_text_field.text = t;
	}
}
