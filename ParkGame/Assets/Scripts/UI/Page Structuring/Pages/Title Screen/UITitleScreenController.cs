using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Managers;

public class UITitleScreenController : MonoBehaviour
{
    [SerializeField] private Button enterButton;
    [SerializeField] private UnityEvent onEnterPressed;
    
    void Start()
    {
        enterButton.onClick.AddListener(Enter);
    }

    async void Enter()
    {
        await SessionManager.Singleton.UnityServicesInitializationTask;

        onEnterPressed.Invoke();
    }
}
