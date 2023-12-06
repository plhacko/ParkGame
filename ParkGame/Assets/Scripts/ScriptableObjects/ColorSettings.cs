using System;
using UnityEngine;

[Serializable]
public struct NamedColor
{
    public string Name;
    public Color Color;
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ColorSettingScriptableObject", order = 1)]
public class ColorSettings : ScriptableObject
{
    public NamedColor[] Colors;
}