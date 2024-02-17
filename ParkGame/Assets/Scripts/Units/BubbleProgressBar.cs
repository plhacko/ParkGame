using UnityEngine;

public class BubbleProgressBar : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] SpriteArray;

    [SerializeField] float MaxValue = 1.0f;
    [SerializeField] float Value = 0.0f;
    
    public Color ColorOff = Color.black;

    private Color cachedColor;
    
    private void Start()
    {
        UpdateProgressBar(ColorOff);
    }

    public void SetMaxValue(float maxValue, Color color)
    {
        MaxValue = maxValue;
        UpdateProgressBar(color);
    }

    public void SetValue(float value, Color color, bool useCached = false)
    {
        Value = value;
        cachedColor = color;

        UpdateProgressBar(useCached ? cachedColor : color);
    }

    public void UpdateProgressBar(Color color)
    {
        float step = MaxValue / SpriteArray.Length;
        int activeBubblesCount = Mathf.RoundToInt(Value / step);

        for (int i = 0; i < activeBubblesCount; i++)
        {
            SpriteArray[i].color = color;
        }
        for (int i = activeBubblesCount; i < SpriteArray.Length; i++)
        {
            SpriteArray[i].color = ColorOff;
        }
    }
}
