using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapErrorMessagePanelUI : MonoBehaviour
{
    [SerializeField] private CreateMapWithOverlay mapWithOverlay;
    [SerializeField] private GameObject root;
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI errorText;

    void Awake()
    {
        root.SetActive(false);
        mapWithOverlay.OnMapCreationErrors += SetErrorMessages;
        closeButton.onClick.AddListener(HidePanel);
    }

    public void SetErrorMessages(List<string> errorMessages)
    {
        errorText.text = "";
        foreach (var message in errorMessages)
        {
            errorText.text += message + "\n";
        }
        root.SetActive(true);
    }
    
    private void HidePanel()
    {
        AudioManager.Instance.PlayClickSFX();
        root.SetActive(false);
    }
}
