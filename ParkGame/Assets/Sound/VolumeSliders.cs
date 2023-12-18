using UnityEngine;
using UnityEngine.UI;

public class VolumeSliders : MonoBehaviour
{
    [SerializeField]
    private Slider notificationsSlider;
    [SerializeField]
    private Slider sfxSlider;
    private void Start() {
        if (!(sfxSlider && notificationsSlider)) {
            return;
        }
        AudioManager.Instance.ChangeSfxVolume(sfxSlider.value);
        AudioManager.Instance.ChangeSfxVolume(notificationsSlider.value);
        sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.ChangeSfxVolume(val));
        notificationsSlider.onValueChanged.AddListener(val => AudioManager.Instance.ChangeNotificationsVolume(val));
    }
}
