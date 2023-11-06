using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UploadMapDialog : MonoBehaviour
{
    [SerializeField] private CreateMapWithOverlay mapWithOverlay;
    [SerializeField] private MapDataFirebaseManager mapDataFirebaseManager;
    [SerializeField] private TMP_InputField mapNameInputField;
    [SerializeField] private Button uploadButton;
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private Button goBackButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject root;
    [SerializeField] private UIPage mainMenu; 
    private void Awake()
    {
        root.SetActive(false);
        returnToMenuButton.gameObject.SetActive(false);
        uploadButton.interactable = true;
        statusText.text = "";
        
        mapDataFirebaseManager.OnMapUploaded.AddListener(() =>
        {
            statusText.text = "Upload successful!";
            returnToMenuButton.gameObject.SetActive(true);
        });
        
        mapDataFirebaseManager.OnMapUploadFailed.AddListener(() =>
        {
            statusText.text = "Upload failed...";
            uploadButton.interactable = true;
        });
        
        uploadButton.onClick.AddListener(() =>
        {
            statusText.text = "Uploading...";
            mapDataFirebaseManager.UploadMapData(mapNameInputField.text);
            uploadButton.interactable = false;
        });
        
        returnToMenuButton.onClick.AddListener(() =>
        {
            uploadButton.interactable = false;
            goBackButton.interactable = false;
            Destroy(mapWithOverlay.GetFetchedMap().gameObject);
            // SceneManager.LoadScene("MainMenu");
            UIController.Singleton.PushUIPage(mainMenu);
        });
        
        goBackButton.onClick.AddListener(() =>
        {
            returnToMenuButton.gameObject.SetActive(false);
            uploadButton.interactable = true;
            statusText.text = "";
            root.SetActive(false);
        });
    }
}
