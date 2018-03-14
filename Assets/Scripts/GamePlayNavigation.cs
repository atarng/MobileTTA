using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using AtRng.MobileTTA;

// This is added to buttons I think... to switch between scenes.
public class GamePlayNavigation : SceneNavigation {
    //public string mapName;
    const string LEVEL_DATA_PATH = "LevelData/";
    public LevelScriptableObject lso;
    public static LevelScriptableObject loadedLevel;

    public override void NavigateToScene() {
        transform.SetParent(null);
        loadedLevel = lso;
        StartCoroutine(OpenLevel());
    }
    
    /// <summary>
    // Corourtine that waits until the leve is done, and then destroys this
    // gameobject upon completion.
    /// </summary>
    private IEnumerator OpenLevel() {
        DontDestroyOnLoad(gameObject);
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(m_sceneName);
        
        while (!asyncLoadLevel.isDone) {
            yield return null;
        }
        Debug.Log("[SceneNavigation/OpenLevel] Done");
        loadedLevel = null;
        Destroy(gameObject);
    }
}
