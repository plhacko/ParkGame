using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIUnit : Selectable, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Slider healthBarSlider;
    [SerializeField] private List<Sprite> unitIcons;
    [SerializeField] private Image unitIcon;
    private ISoldier unit;
    [SerializeField] private float pressHoldTimeSuccess = 1f;
    private Stopwatch pressHoldTimer = new Stopwatch();   
    private Action removeAction;
    private Action onDeath; 

    // Update is called once per frame
    void Update()
    {
        if (unit == null)
        {
            return;
        }
        healthBarSlider.value = ((float)unit.HP) / unit.MaxHP;

        if (pressHoldTimer.ElapsedMilliseconds / 1000f > pressHoldTimeSuccess)
        {
            UnityEngine.Debug.Log("Remove unit");
            if (IsInteractable())
                removeAction?.Invoke();
            pressHoldTimer.Reset();
            pressHoldTimer.Stop();
        }
    }

    protected override void OnDestroy()
    {
        if (unit == null)
        {
            return;
        }
        unit.OnDeath -= onDeath;
        base.OnDestroy();
    }   

    public void Initialize(ISoldier unit, Action removeAction, Action onDeath)
    {
        this.unit = unit;
        this.removeAction = removeAction;
        this.onDeath = onDeath;
        unit.OnDeath += this.onDeath;
        
        if (unitIcons.Count == Enum.GetNames(typeof(ISoldier.UnitType)).Length)
        {
            var type = unit.Type;
            unitIcon.sprite = unitIcons[(int)type];
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        pressHoldTimer.Stop();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        pressHoldTimer.Restart();
    }
}
