using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Canvas))]
[DisallowMultipleComponent]
public class UIController : MonoBehaviour
{
    public static UIController Singleton { get; private set; }
    public UIPage initialPage;
    [SerializeField]
    private GameObject firstFocusItem;
    private Canvas canvas;

    private Stack<UIPage> pageStack = new Stack<UIPage>();
#if UNITY_EDITOR
    public List<string> PageStackNames = new List<string>();
#endif
    [SerializeField] private GameObject popUpScreen;
    public string TopStackPageName { 
        get {
            if (pageStack.Count == 0) { return ""; }
            return pageStack.Peek().name; 
        } 
    }
    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        foreach (RectTransform child in transform)
        {
            UIPage page = child.GetComponent<UIPage>();
            if (page != null)
            {
                page.Prepare();
            }
        }

        if (initialPage != null)
        {
            if (firstFocusItem != null)
            {
                EventSystem.current.SetSelectedGameObject(firstFocusItem);
            }
        }

        if (initialPage != null)
        {
            PushUIPage(initialPage);
        }
    }

    public void PushUIPage(UIPage page)
    {
        page.Enter(true);

        if (pageStack.Count > 0)
        {
            UIPage currentPage = pageStack.Peek();

            if (page.ExitOnNextPage)
            {
                currentPage.Exit(false);
                pageStack.Pop();
#if UNITY_EDITOR
                PageStackNames.Remove(currentPage.name);
#endif
            }
        }

        pageStack.Push(page);
#if UNITY_EDITOR
        PageStackNames.Add(page.name);
#endif
    }

    public void PopUIPage()
    {
        if (pageStack.Count < 1)
        {
            Debug.LogError("Cannot pop empty page stack");
            return;
        }

        UIPage currentPage = pageStack.Pop();
#if UNITY_EDITOR
        PageStackNames.Remove(currentPage.name);
#endif
        currentPage.Exit(true);

        if (pageStack.Count > 0)
        {        
            UIPage newPage = pageStack.Peek();
            newPage.Enter(false);
        }
    }

    public void ShowPopUp(string label, string message, string dismissButtonText = "Dismiss", Action dismissAction = null, string popUpName = "Pop Up Screen")
    {
        if (popUpScreen == null)
        {
            Debug.LogError("No pop up screen found");
            return;
        }

        var go = Instantiate(popUpScreen, transform);

        go.name = popUpName;

        var rectTransform = go.GetComponent<RectTransform>();
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;

        UIPopUpScreenController popUp = go.GetComponent<UIPopUpScreenController>();
        popUp.PopUpTitle = label;
        popUp.TextMessage = message;
        popUp.ButtonDismissAction = dismissAction;
        popUp.buttonDismissText = dismissButtonText;
 
        var page = go.GetComponent<UIPage>();

        PushUIPage(page);
    }
}
