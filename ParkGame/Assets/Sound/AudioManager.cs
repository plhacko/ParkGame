using System;
using UnityEngine;
using System.Collections.Generic;



public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class Sound {
        public string name;
        public AudioClip sound;
    }

    public static AudioManager Instance;
    [SerializeField]
    private AudioSource sfxSource;
    [SerializeField]
    private List<Sound> sfxList;
    private bool sfxMute;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(AudioClip sfx) {
        sfxSource.PlayOneShot(sfx);
    }

    public void PlaySFX(string sfxName) {
        foreach (Sound s in sfxList) {
            if (s.name == sfxName) {
                sfxSource.PlayOneShot(s.sound);
                return;
            }
        }
    }

    public void ChangeSfxVolume(float volume) {
        sfxSource.volume = volume;
    }

    public void ToggleSfx() {
        sfxSource.mute = !sfxMute;
    }
}
