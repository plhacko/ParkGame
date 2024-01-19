using System.Collections.Generic;
using UnityEngine;

public class UITabGroupImage : MonoBehaviour
{
    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private GameObject panelsParent;
    [Min(0)]
    [SerializeField] private int defaultTabIndex = 0;
    
    private readonly List<(UITabButtonImage button, GameObject panel)> tabs = new();
    
    public void Start()
    {
        var buttons = buttonsParent.GetComponentsInChildren<UITabButtonImage>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if(panelsParent.transform.childCount <= i)
            {
                Debug.LogWarning("Tab buttons and panels count mismatch");
                return;
            }
            
            var panel = panelsParent.transform.GetChild(i).gameObject;
            var button = buttons[i];
            button.SetTabGroup(this);
            
            tabs.Add((button, panel));
        }

        if (defaultTabIndex < 0 || defaultTabIndex >= tabs.Count)
        {
            Debug.LogError("Default tab index out of range");
            return;
        }

        OnTabSelected(tabs[defaultTabIndex].button);
    }
    
    public void OnTabSelected(UITabButtonImage clickedButton)
    {
        foreach (var (button, panel) in tabs)
        {
            bool active = button == clickedButton;
            panel.SetActive(active);
            button.interactable = !active;
        }
    }
}
