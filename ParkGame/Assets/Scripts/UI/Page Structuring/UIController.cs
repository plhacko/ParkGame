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
    [SerializeField]
    private UIPage initialPage;
    [SerializeField]
    private GameObject firstFocusItem;
    private Canvas canvas;

    private Stack<UIPage> pageStack = new Stack<UIPage>();

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

            if (currentPage.ExitOnNextPage)
            {
                currentPage.Exit(false);
            }
        }

        pageStack.Push(page);
    }

    public void PopUIPage()
    {
        if (pageStack.Count > 1)
        {
            UIPage currentPage = pageStack.Pop();
            currentPage.Exit(true);

            UIPage newPage = pageStack.Peek();
            
            if (newPage.ExitOnNextPage)
            {
                newPage.Enter(false);
            }
            
        }
        else 
        {
            Debug.LogError("Cannot pop the last page");
        }
    }
}
