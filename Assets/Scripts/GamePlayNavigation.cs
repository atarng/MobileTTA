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
    //public Fader_MB m_fader_go;
    private Fader_MB fader_instance = null;

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
        fader_instance = Fader_MB.CreateFader();
        fader_instance.FadeInTime = 1f;
        fader_instance.FadeOutTime = 1f;
        fader_instance.FadeColor = new Color(0,0,0);
        fader_instance.BeginFadeIn();

        while (fader_instance.IsFading) yield return null;

        DontDestroyOnLoad(gameObject);
        AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(m_sceneName);
        
        while (!asyncLoadLevel.isDone) {
            yield return null;
        }
        Debug.Log("[SceneNavigation/OpenLevel] Done");
        loadedLevel = null;

        yield return new WaitForSeconds(1);


        fader_instance.BeginFadeOut();
        while (fader_instance.IsFading) yield return null;

        Destroy(fader_instance.gameObject);
        Destroy(gameObject);
    }
}
