using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField]
    private AudioSource sfxSource;

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
        if (sfxSource.isPlaying) {
            Debug.Log("is playing");
        }
        sfxSource.PlayOneShot(sfx);
        Debug.Log("play sfx");

    }

    public void ChangeSfxVolume(float volume) {
        sfxSource.volume = volume;
    }

    public void ToggleSfx() {
        sfxSource.mute = !sfxMute;
    }
}
