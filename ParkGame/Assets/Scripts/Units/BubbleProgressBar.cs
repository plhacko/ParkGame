using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngineInternal;

public class BubbleProgressBar : MonoBehaviour, IProgressBar
{
    [SerializeField] SpriteRenderer[] SpriteArray;

    [SerializeField] float MaxValue = 1.0f;
    [SerializeField] float Value = 0.0f;

    public Color ColorOn = Color.red;
    public Color ColorOff = Color.black;

    private void Start()
    {
        UpdateProgressBar();
    }

    public void SetMaxValue(float maxValue)
    {
        MaxValue = maxValue;
        UpdateProgressBar();
    }

    public void SetValue(float value)
    {
        Value = value;
        UpdateProgressBar();
    }

    public void UpdateProgressBar()
    {
        float step = MaxValue / SpriteArray.Length;
        int activeBubblesCount = Mathf.RoundToInt(Value / step);

        for (int i = 0; i < activeBubblesCount; i++)
        {
            SpriteArray[i].color = ColorOn;
        }
        for (int i = activeBubblesCount; i < SpriteArray.Length; i++)
        {
            SpriteArray[i].color = ColorOff;
        }
    }
}
