using UnityEngine;

public class ResetSoundSettings : MonoBehaviour
{
    private void Start() {
        AudioManager.Instance.ResetSoundSettings();
    }
}
