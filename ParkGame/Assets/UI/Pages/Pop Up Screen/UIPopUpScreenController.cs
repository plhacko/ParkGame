using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class UIPopUpScreenController : UIPageController
{
    [SerializeField] private TextMeshProUGUI popUpTitleText;
    [SerializeField] private TextMeshProUGUI textMessageText;
    [SerializeField] private Button buttonDismiss;

    public string PopUpTitle {
        get { return popUpTitleText.text; }
        set { popUpTitleText.text = value; }
    }

    public string TextMessage {
        get { return textMessageText.text; }
        set { textMessageText.text = value; }
    }

    private void Awake()
    {
        buttonDismiss.onClick.AddListener(() => UIController.Singleton.PopUIPage());
    }

    public Action ButtonDismissAction {
        set 
        { 
            if (value == null)
            { 
                buttonDismiss.onClick.RemoveAllListeners();
                buttonDismiss.onClick.AddListener(() => UIController.Singleton.PopUIPage());
            }
            else { buttonDismiss.onClick.AddListener(() => value()); }  
        }
    }

    public string buttonDismissText {
        get { return buttonDismiss.GetComponentInChildren<TextMeshProUGUI>().text; }
        set { buttonDismiss.GetComponentInChildren<TextMeshProUGUI>().text = value; }
    }

    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
        Clear();
    }

    private void Clear()
    {
        PopUpTitle = "";
        TextMessage = "";
        buttonDismissText = "";
        ButtonDismissAction = null;
    }
}
