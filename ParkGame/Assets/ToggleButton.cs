using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    private Button button;
    private uint MaxStates;
    public uint CurrentState { get; private set; } = 0;
    private List<UnityAction> onClicks = new List<UnityAction>();
    void Start()
    {
        button = GetComponent<Button>();       
        MaxStates = (uint)onClicks.Count;
        button.onClick.AddListener(OnClick);
        var nextState = (CurrentState + 1) % MaxStates;
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = nextState.ToString();
    }
    
    void OnClick() {
        CurrentState = (CurrentState + 1) % MaxStates;
        var nextState = (CurrentState + 1) % MaxStates;
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = nextState.ToString();
        onClicks[(int)CurrentState].Invoke();
    }

    public void AddListener(UnityAction onClick) {
        onClicks.Add(onClick);
    }
}
