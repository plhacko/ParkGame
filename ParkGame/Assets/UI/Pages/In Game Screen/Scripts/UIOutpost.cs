using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Firebase.Auth;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

public class UIOutpost : Selectable, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI pawnCount;
    [SerializeField] private TextMeshProUGUI archerCount;
    [SerializeField] private TextMeshProUGUI horsemanCount;
    [SerializeField] private Image outpostIcon;
    [SerializeField] private List<Sprite> outpostIcons;
    [SerializeField] private Sprite castleIcon;
    
    private Outpost outpost;
    private ToggleSpawner spawner;
    [SerializeField] private float holdTimerLimit = 1.0f;
    Stopwatch stopwatch = new Stopwatch();
    private Action removeAction;
    
    public void Initialize(Outpost outpost, Action removeAction)
    {
        this.outpost = outpost;
        spawner = outpost.GetComponent<ToggleSpawner>();
        this.removeAction = removeAction;
        OnUnitTypeChange(spawner.OutpostUnitType);
        spawner.OnUnitTypeChange += OnUnitTypeChange;
        OnUnitTypeCountChange();
        outpost.OnUnitTypeCountChange += OnUnitTypeCountChange;
        this.outpost.RegisterOnTeamChange(OnTeamChange);
    }

    private void OnTeamChange()
    {
        removeAction?.Invoke();
    }

    protected override void OnDestroy()
    {
        if (outpost == null)
            return;

        spawner.OnUnitTypeChange -= OnUnitTypeChange;
        outpost.OnUnitTypeCountChange -= OnUnitTypeCountChange;
        outpost.UnregisterOnTeamChange(OnTeamChange);
        base.OnDestroy();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        stopwatch.Restart();
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        stopwatch.Stop();
    }

    private void OnUnitTypeChange(ISoldier.UnitType type)
    {
        if (outpost.IsCastle)
        {
            outpostIcon.sprite = castleIcon;
            return;
        }
        if (outpostIcons.Count != Enum.GetNames(typeof(ISoldier.UnitType)).Length)
            return;

        var newTypeSprite = outpostIcons[(int)type];
        if (newTypeSprite == null)
            return;

        outpostIcon.sprite = newTypeSprite;
    }

    private void OnUnitTypeCountChange()
    {
        var unitTypeCount = outpost.UnitTypeCount;
        pawnCount.text = unitTypeCount[ISoldier.UnitType.Pawn].ToString();
        archerCount.text = unitTypeCount[ISoldier.UnitType.Archer].ToString();
        horsemanCount.text = unitTypeCount[ISoldier.UnitType.Horseman].ToString();
    }

    void Update()
    {
        // Hold
        if (stopwatch.IsRunning && stopwatch.ElapsedMilliseconds / 1000f > holdTimerLimit)
        {
            if (IsInteractable())
                GameManager.Instance.PanTo(outpost.transform.position, 0.33f);
            stopwatch.Stop();
            stopwatch.Reset();
        }
        // Tap
        if (!stopwatch.IsRunning && stopwatch.ElapsedMilliseconds > 0.0f && stopwatch.ElapsedMilliseconds / 1000f <= holdTimerLimit)
        {
            if (!outpost.IsCastle)
            {
                if (IsInteractable())
                    outpost.RequestChangingSpawnTypeServerRpc(NetworkManager.Singleton.LocalClientId, true);
            }
            stopwatch.Stop();
            stopwatch.Reset();
        }
    }        
}
