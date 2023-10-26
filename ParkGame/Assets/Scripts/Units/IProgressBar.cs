using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IProgressBar
{
    public void SetValue(float value);
    public void SetMaxValue(float maxValue);
}
