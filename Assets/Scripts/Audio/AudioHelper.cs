using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioHelper : MonoBehaviour {
    [SerializeField]
    private AudioSource m_audioSource;

    public void PlayOneShot(AudioClip audio_clip, float volume = 1) {
        m_audioSource.clip = audio_clip;
        m_audioSource.volume = volume;

        StartCoroutine(TerminateOnCompletion());
    }

    private IEnumerator TerminateOnCompletion() {
        m_audioSource.Play();
        while (m_audioSource.isPlaying) {
            yield return null;
        }
        Destroy(gameObject);
    }

}
