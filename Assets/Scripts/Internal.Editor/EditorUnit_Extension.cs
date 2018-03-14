using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
public class EditorUnit_Extension : MonoBehaviour {
    public EditorUnit editorUnit;
    public InputField inputField;
    private const int DEFAULT = 10;
    private void Start() {
        editorUnit.ReadDefinitionID(DEFAULT);
        inputField.text = DEFAULT.ToString();

        inputField.onEndEdit.AddListener(delegate {
            int test = DEFAULT;
            int.TryParse(inputField.text, out test);
            //editorUnit.UpdateDefinitionID(test);
            editorUnit.ReadDefinitionID(test);
        });
    }
}
#endif