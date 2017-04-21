using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using AtRng.MobileTTA;

public class GamePlayNavigation : SceneNavigation {
    //public string mapName;
    const string LEVEL_DATA_PATH = "LevelData/";
    public LevelScriptableObject lso;
    public static LevelScriptableObject loadedLevel;

    public override void NavigateToScene() {
        //Debug.Log("[SceneNavigation/NavigateToScene]");
        transform.SetParent(null);
        loadedLevel = lso;
        StartCoroutine(OpenLevel());
    }

    private IEnumerator OpenLevel() {
        //Debug.Log("[SceneNavigation/OpenLevel] mapName: " + mapName);
        DontDestroyOnLoad(gameObject);
        //LevelScriptableObject lso = (LevelScriptableObject)Resources.Load(LEVEL_DATA_PATH + mapName);

        //Debug.Log("[SceneNavigation/OpenLevel] StartSceneLoad. lso: " + lso.name);

        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(m_sceneName);
        
        while (!asyncLoadLevel.isDone) {
            yield return null;
        }
        Debug.Log("[SceneNavigation/OpenLevel] Done");
        /*
        GameManager gm = SingletonMB.GetInstance<GameManager>();
        if (gm != null) {
            Debug.Log("[SceneNavigation/OpenLevel] GM is accessible!");
            gm.LevelInitData = lso;
        }
        */
        loadedLevel = null;

        Destroy(gameObject);
    }
}
