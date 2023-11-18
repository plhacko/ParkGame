using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour {
    private enum VolumeType {
        MASTER,
        MUSIC,
        UI,
        SFX
    }

    [Header("Type")]
    [SerializeField] private VolumeType volumeType;

    private Slider volumeSlider;

    private void Awake() {
        volumeSlider = this.GetComponentInChildren<Slider>();
    }

    private void Update() {
        switch (volumeType) {
            case VolumeType.MASTER:
                volumeSlider.value = AudioManager.Instance.masterVolume;
                break;
            case VolumeType.MUSIC:
                volumeSlider.value = AudioManager.Instance.musicVolume;
                break;
            case VolumeType.UI:
                volumeSlider.value = AudioManager.Instance.UIVolume;
                break;
            case VolumeType.SFX:
                volumeSlider.value = AudioManager.Instance.SFXVolume;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
    }

    public void OnSliderValueChanged() {
        switch (volumeType) {
            case VolumeType.MASTER:
                AudioManager.Instance.masterVolume = volumeSlider.value;
                break;
            case VolumeType.MUSIC:
                AudioManager.Instance.musicVolume = volumeSlider.value;
                break;
            case VolumeType.UI:
                AudioManager.Instance.UIVolume = volumeSlider.value;
                break;
            case VolumeType.SFX:
                AudioManager.Instance.SFXVolume = volumeSlider.value;
                break;
            default:
                Debug.LogWarning("Volume Type not supported: " + volumeType);
                break;
        }
    }
}