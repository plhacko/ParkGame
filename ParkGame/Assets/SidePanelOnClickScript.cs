using UnityEngine;
using UnityEngine.UI;

public class SidePanelOnClickScript : MonoBehaviour
{
    // hot fix solution for side panels in drawing map scene
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private Button button3;
    [SerializeField] private Button button4;
    [SerializeField] private Toggle toggle4;
    [SerializeField] private Button button5;
    [SerializeField] private Button button6;
    [SerializeField] private Button button7;
    [SerializeField] private Button button8;
    void OnEnable() {
        button1.onClick.AddListener(AudioManager.Instance.PlayClickSFX);
        button2.onClick.AddListener(AudioManager.Instance.PlayClickSFX);
        button3.onClick.AddListener(AudioManager.Instance.PlayClickSFX);
        if (button4) { button4.onClick.AddListener(AudioManager.Instance.PlayClickSFX); }
        if (toggle4) { toggle4.onValueChanged.AddListener(delegate { AudioManager.Instance.PlayClickSFX(); }); }
        if (button5) { button5.onClick.AddListener(AudioManager.Instance.PlayClickSFX); }
        if (button6) { button6.onClick.AddListener(AudioManager.Instance.PlayClickSFX); }
        if (button7) { button7.onClick.AddListener(AudioManager.Instance.PlayClickSFX); }
        if (button8) {button8.onClick.AddListener(AudioManager.Instance.PlayClickSFX); }
    }
}
