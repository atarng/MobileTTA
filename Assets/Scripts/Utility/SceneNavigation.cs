using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour {
    public string m_sceneName;
    public void NavigateToScene() {// string sceneName) {
        SceneManager.LoadScene(m_sceneName);
    }
}
