using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    struct State {
        public string name;
        public UnityAction onClick;
    }

    private Button button;
    private uint MaxStates;
    public uint CurrentState { get; private set; } = 0;
    private List<State> states = new List<State>();
    void Start()
    {
        button = GetComponent<Button>();       
        MaxStates = (uint)states.Count;
        button.onClick.AddListener(OnClick);
        var nextState = (CurrentState + 1) % MaxStates;
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = states[(int)nextState].name;
    }
    
    void OnClick() {
        CurrentState = (CurrentState + 1) % MaxStates;
        var nextState = (CurrentState + 1) % MaxStates;
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = states[(int)nextState].name;
        states[(int)CurrentState].onClick.Invoke();
    }

    public void AddListener(string name, UnityAction onClick) {
        states.Add(new State { name = name, onClick = onClick });
    }
}
