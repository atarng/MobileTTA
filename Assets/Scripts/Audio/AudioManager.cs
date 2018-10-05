using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtRng.MobileTTA {

    public class AudioManager : SingletonMB, ISoundManager
    {
        [Serializable]
        private struct AudioKeyMap {
            public string m_key;
            public AudioClip m_clip;
            public float m_audio_volume;
        }

        [SerializeField]
        private AudioHelper m_audio_prefab;

        [SerializeField]
        AudioKeyMap[] m_tempSoundArray;
        Dictionary<string, AudioKeyMap> m_audio_dictionary;

        protected override void OnAwake() {
            if (m_audio_dictionary == null) {
                m_audio_dictionary = new Dictionary<string, AudioKeyMap>();
            }
            for (int i = 0; i < m_tempSoundArray.Length ; i++) {
                m_audio_dictionary.Add(m_tempSoundArray[i].m_key, m_tempSoundArray[i]);
            }
            base.OnAwake();
        }

        public void PlaySound(string soundKey)
        { 
            switch (soundKey) {
                // All other cases are for special case.
                case "Combat1":
                case "Combat2": {
                    FireOneOff(m_audio_dictionary["Combat"]);
                    break;
                }
                default:
                    if (m_audio_dictionary.ContainsKey(soundKey)) {
                        FireOneOff(m_audio_dictionary[soundKey]);
                    } else {
                        Debug.LogWarning(string.Format("Could Not Find SoundKey: {0}", soundKey));
                    }
                    break;
            }
        }

        private void FireOneOff(AudioKeyMap audio_node) {
            AudioHelper audio_helper = Instantiate<AudioHelper>(m_audio_prefab);
            audio_helper.PlayOneShot(audio_node.m_clip, audio_node.m_audio_volume);
        }
    }
}
