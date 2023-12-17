using UnityEngine;
using UnityEngine.UI;

public class VolumeSliders : MonoBehaviour
{
    [SerializeField]
    private Slider sfxSlider;
    private void Start() {
        AudioManager.Instance.ChangeSfxVolume(sfxSlider.value);
        sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.ChangeSfxVolume(val));
    }
}
