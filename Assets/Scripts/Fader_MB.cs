using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fader_MB : MonoBehaviour
{
    [SerializeField]
    private Image m_image;
    //private Color m_color;
    public Color FadeColor
    {
        get {
            return m_image.color;
        }
        set {
            m_image.color = value;
        }
    }

    private float m_fade_in;
    public float FadeInTime
    {
        get { return m_fade_in; }
        set { m_fade_in = value; }
    }
    private float m_fade_out;
    public float FadeOutTime
    {
        get { return m_fade_out; }
        set { m_fade_out = value; }
    }

    bool m_isFading = false;
    public bool IsFading {
        get { return m_isFading; }
        protected set { m_isFading = value; }
    }

    public void BeginFadeIn()
    {
        Debug.Log("BeginFadeIn");
        IsFading = true;
        StartCoroutine(Fade(true));
    }
    public void BeginFadeOut()
    {
        Debug.Log("BeginFadeOut");

        IsFading = true;
        StartCoroutine(Fade(false));
    }

    private IEnumerator Fade(bool fade_in)
    {
        float elapsed_time = 0;
        float target_time = (fade_in) ? FadeInTime : FadeOutTime;
        while (elapsed_time < target_time) {
            float interpolation = fade_in ? (elapsed_time / target_time) : 1 - (elapsed_time / target_time);

            FadeColor = new Color(FadeColor.r, FadeColor.g, FadeColor.b, interpolation);


            elapsed_time += Time.deltaTime;
            yield return null;
        }
        IsFading = false;
    }

    public static Fader_MB CreateFader() {
        GameObject go = Instantiate(Resources.Load("Prefabs/Fader")) as GameObject;
        Fader_MB f_mb = go.GetComponent<Fader_MB>();
        if (f_mb != null)
        {
            DontDestroyOnLoad(f_mb.gameObject);
        }
        else {
            Debug.LogWarning("Could Not Instantiate Fade Prefab.");
        }
        return f_mb;
    }
}
