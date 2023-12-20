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
        
        foreach (RectTransform child in transform)
        {
            child.GetComponent<UIPage>().Prepare();
        }   

        if (!ServicesManager.Instance.AreInitializedUnityServices() || !ServicesManager.Instance.IsSignedToUnityAuth())
        {
            page = titlePage;
        }
        else
        {
            page = mainMenuPage;
        }

        uIController.PushUIPage(page);
    }
}
