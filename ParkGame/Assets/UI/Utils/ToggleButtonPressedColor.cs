using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtonPressedColor : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI text;
    [SerializeField] public string pressedText;

    private string normalText;
    private Color normalColor;
    private Color highlightedColor;
    private Toggle toggle;

    private void Start()
    {
        normalText = text.text;
        toggle = GetComponent<Toggle>();
        normalColor = toggle.colors.normalColor;
        highlightedColor = toggle.colors.highlightedColor;
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        ColorBlock cb = toggle.colors;
        if (isOn)
        {
            cb.normalColor = highlightedColor;
            cb.highlightedColor = highlightedColor;
            cb.selectedColor = highlightedColor;
            text.text = pressedText;
        }
        else
        {
            cb.normalColor = normalColor;
            cb.highlightedColor = normalColor;
            cb.selectedColor = normalColor;
            text.text = normalText;
        }

        toggle.colors = cb;
    }
}
