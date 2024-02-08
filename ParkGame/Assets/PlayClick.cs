using UnityEngine;

public class PlayClick : MonoBehaviour {
    AudioManager instance;
    void Start() {
        instance = FindObjectOfType<AudioManager>();
    }

    public void PlayClickSfx() {
        instance.PlayClickSFX();
    }
}
