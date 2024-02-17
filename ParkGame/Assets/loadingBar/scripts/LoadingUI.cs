using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour {

    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI textStatus;
    [SerializeField] private TextMeshProUGUI textPercent;
    [SerializeField] private GameObject root;
    
    void Awake () {
        image.fillAmount = 0.0f;
    }
    
    public void SetProgress(float progress, string status)
    {
        image.fillAmount = progress;
        textPercent.text = (progress * 100).ToString("0") + "%";
        textStatus.text = status;
        root.SetActive(true);
    }

    public void Show(bool show)
    {
        root.SetActive(show);
    }
}
