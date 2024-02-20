using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Serializable]
public struct StateImage {
    public Sprite Sprite;
    public UnityEvent OnClick;
}

public class ToggleButtonImage : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private List<StateImage> states = new();
    
    private Button button;
    private uint MaxStates;
    public uint CurrentState { get; private set; } = 0;

    void Start()
    {
        button = GetComponent<Button>();       
        MaxStates = (uint)states.Count;
        button.onClick.AddListener(OnClick);
        var nextState = (CurrentState + 1) % MaxStates;
        targetImage.sprite = states[(int)nextState].Sprite;
    }
    
    void OnClick() {
        CurrentState = (CurrentState + 1) % MaxStates;
        var nextState = (CurrentState + 1) % MaxStates;
        targetImage.sprite = states[(int)nextState].Sprite;
        states[(int)CurrentState].OnClick.Invoke();
    }

    // kinda hack but w/e
    public void SwitchState(uint stateIndex)
    {
        if(CurrentState == stateIndex) return;
        
        CurrentState = stateIndex;
        var nextState = (CurrentState + 1) % MaxStates;
        targetImage.sprite = states[(int)nextState].Sprite;
    }
}
