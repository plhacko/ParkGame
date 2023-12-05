using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUnit : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private List<Texture2D> unitIcons;
    private Image unitIcon;
    private Soldier unit;
    [SerializeField] private float pressHoldTimeSuccess = 1f;
    private Stopwatch pressHoldTimer = new Stopwatch();   
    private Action removeAction; 

    // Update is called once per frame
    void Update()
    {
        if (unit == null)
        {
            return;
        }
        healthBarSlider.value = ((float)unit.HP) / unit.MaxHP;
    }

    public void Initialize(Soldier unit, Action removeAction)
    {
        this.unit = unit;
        this.removeAction = removeAction;
        
        if (unitIcons.Count == Enum.GetNames(typeof(Soldier.UnitType)).Length)
        {
            var type = unit.Type;
            unitIcon.sprite = Sprite.Create(unitIcons[(int)type], new Rect(0, 0, unitIcons[(int)type].width, unitIcons[(int)type].height), new Vector2(0.5f, 0.5f));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressHoldTimer.Stop();
        var time = pressHoldTimer.ElapsedMilliseconds / 1000f; 
        if (time > pressHoldTimeSuccess)
        {
            UnityEngine.Debug.Log("Remove unit");
            removeAction?.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressHoldTimer.Restart();
    }
}
