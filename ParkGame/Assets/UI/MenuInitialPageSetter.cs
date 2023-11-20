using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInitialPageSetter : MonoBehaviour
{
    public UIController uIController;
    [SerializeField] private UIPage titlePage;
    [SerializeField] private UIPage mainMenuPage;
    [SerializeField] private UIPage welcomePage;


    // Start is called before the first frame update
    void Start()
    {
        UIPage page;
        if (ServicesManager.Instance.State < ServiceType.UnityServices)
        {
            page = titlePage;   
        }
        else if (ServicesManager.Instance.State < ServiceType.UnityAuth)
        {
            page = welcomePage;
        }
        else
        {
            page = mainMenuPage;
        }

        uIController.PushUIPage(page);
    }
}
