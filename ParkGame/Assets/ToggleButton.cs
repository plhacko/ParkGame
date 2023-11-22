using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    private Button button;
    [SerializeField] private uint States = 2;
    private uint CurrentState = 0;
    [SerializeField] private List<UnityEvent> Events;
    void Start()
    {
        button = GetComponent<Button>();       
        if (States != Events.Count) {
            Debug.LogError("States and Events must be the same length");
        }
        button.onClick.AddListener(OnClick);
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = CurrentState.ToString();
    }
    
    void OnClick() {
        CurrentState = (CurrentState + 1) % States;
        button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = CurrentState.ToString();
        Events[(int)CurrentState].Invoke();
    }
}
