using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MapDrawingModeSwitcher : MonoBehaviour
{
    // Start is called before the first frame update

    [System.Serializable]
    public struct MapDrawingMode
    {
        public string name;
        public UnityEvent onEnter;
        public UnityEvent onExit;
    }

    [SerializeField] private List<MapDrawingMode> modes = new List<MapDrawingMode>();

    private Button button;
    private TMPro.TextMeshProUGUI text;
    private uint MaxStates;
    public uint CurrentState { get; private set; } = 0;
    void Start()
    {
        button = GetComponent<Button>();
        text = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        MaxStates = (uint)modes.Count;
        button.onClick.AddListener(OnClick);
        text.text = modes[(int)CurrentState].name;
    }

    void OnClick()
    {
        modes[(int)CurrentState].onExit.Invoke();
        CurrentState = (CurrentState + 1) % MaxStates;
        text.text = modes[(int)CurrentState].name; 
        modes[(int)CurrentState].onEnter.Invoke();
    }
}
