using Firebase.Auth;
using TMPro;
using UnityEngine;

public class NameUI : MonoBehaviour
{
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        text.text = FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName ?? "Name not found";
    }
}
