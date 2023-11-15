using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UITabGroup : MonoBehaviour
{
    private List<UITabButton> tabButtons;
    private List<GameObject> tabPanels;
    [SerializeField] private GameObject buttonsParent;
    [SerializeField] private GameObject panelsParent;
    private UITabButton selectedTab;
    [Min(0)]
    [SerializeField] private int defaultTabIndex = 0;

    public void Start()
    {
        tabButtons = new List<UITabButton>();
        foreach (Transform child in buttonsParent.transform)
        {
            tabButtons.Add(child.GetComponent<UITabButton>());
        }

        tabPanels = new List<GameObject>();
        foreach (Transform child in panelsParent.transform)
        {
            tabPanels.Add(child.gameObject);
        }

        if (tabButtons.Count != tabPanels.Count)
        {
            Debug.LogError("Tab buttons and panels count mismatch");
            return;
        }

        foreach (UITabButton button in tabButtons)
        {
            button.SetTabGroup(this);
        }

        if (defaultTabIndex < 0 || defaultTabIndex >= tabButtons.Count)
        {
            Debug.LogError("Default tab index out of range");
            return;
        }

        OnTabSelected(tabButtons[defaultTabIndex]);
    }

    public void Subscribe(UITabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<UITabButton>();
        }
        tabButtons.Add(button);
    }

    public void OnTabEnter(UITabButton button)
    {
        ResetTabs();
        if (selectedTab == null || button != selectedTab)
        {
            button.background.color = button.hoverColor;
        }
    }

    public void OnTabExit(UITabButton button)
    {
        ResetTabs();
    }

    public void OnTabSelected(UITabButton button)
    {
        selectedTab = button;
        ResetTabs();
        button.background.color = button.selectedColor;
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i < tabPanels.Count; i++)
        {
            if (i == index)
            {
                tabPanels[i].SetActive(true);
            }
            else
            {
                tabPanels[i].SetActive(false);
            }
        }
    }

    public void ResetTabs()
    {
        foreach (UITabButton button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab) { continue; }
            button.background.color = button.defaultColor;
        }
    }

}
