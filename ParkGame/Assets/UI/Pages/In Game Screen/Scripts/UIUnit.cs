using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUnit : MonoBehaviour
{
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private List<Texture2D> unitIcons;
    private Image unitIcon;
    private Soldier unit;

    // Update is called once per frame
    void Update()
    {
        if (unit == null)
        {
            return;
        }
        healthBarSlider.value = ((float)unit.HP) / unit.MaxHP;
    }

    public void Initialize(Soldier unit)
    {
        this.unit = unit;
        
        if (unitIcons.Count == Enum.GetNames(typeof(Soldier.UnitType)).Length)
        {
            var type = unit.Type;
            unitIcon.sprite = Sprite.Create(unitIcons[(int)type], new Rect(0, 0, unitIcons[(int)type].width, unitIcons[(int)type].height), new Vector2(0.5f, 0.5f));
        }
    }
}
