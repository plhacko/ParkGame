using UnityEngine;

public class ResetSoundSettings : MonoBehaviour
{
    private void Awake() {
        AudioManager.Instance.ResetSoundSettings();
    }
}
