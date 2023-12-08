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

public class UIOutpost : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private TextMeshProUGUI pawnCount;
    [SerializeField] private TextMeshProUGUI archerCount;
    [SerializeField] private TextMeshProUGUI horsemanCount;
    [SerializeField] private Image outpostIcon;
    [SerializeField] private List<Sprite> outpostIcons;
    [SerializeField] private Sprite castleIcon;
    
    private Outpost outpost;
    [SerializeField] private float holdTimerLimit = 1.0f;
    Stopwatch stopwatch = new Stopwatch();
    private Action removeAction;
    
    public void Initialize(Outpost outpost, Action removeAction)
    {
        this.outpost = outpost;
        this.removeAction = removeAction;
        OnUnitTypeChange(outpost.OutpostUnitType);
        outpost.OnUnitTypeChange += OnUnitTypeChange;
        OnUnitTypeCountChange();
        outpost.OnUnitTypeCountChange += OnUnitTypeCountChange;
        this.outpost.RegisterOnTeamChange(OnTeamChange);
    }

    private void OnTeamChange()
    {
        removeAction?.Invoke();
    }

    private void OnDestroy()
    {
        if (outpost == null)
            return;

        outpost.OnUnitTypeChange -= OnUnitTypeChange;
        outpost.OnUnitTypeCountChange -= OnUnitTypeCountChange;
        outpost.UnregisterOnTeamChange(OnTeamChange);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        stopwatch.Restart();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        stopwatch.Stop();
    }

    private void OnUnitTypeChange(Soldier.UnitType type)
    {
        if (outpost.IsCastle)
        {
            outpostIcon.sprite = castleIcon;
            return;
        }

        if (outpostIcons.Count != Enum.GetNames(typeof(Soldier.UnitType)).Length)
            return;

        var newTypeSprite = outpostIcons[(int)type];

        if (newTypeSprite == null)
            return;

        outpostIcon.sprite = newTypeSprite;
    }

    private void OnUnitTypeCountChange()
    {
        var unitTypeCount = outpost.UnitTypeCount;
        pawnCount.text = unitTypeCount[Soldier.UnitType.Pawn].ToString();
        archerCount.text = unitTypeCount[Soldier.UnitType.Archer].ToString();
        horsemanCount.text = unitTypeCount[Soldier.UnitType.Horseman].ToString();
    }

    void Update()
    {
        // Hold
        if (stopwatch.IsRunning && stopwatch.ElapsedMilliseconds / 1000f > holdTimerLimit)
        {
            UnityEngine.Debug.Log("Hold");
        }
        // Tap
        if (!stopwatch.IsRunning && stopwatch.ElapsedMilliseconds > 0.0f && stopwatch.ElapsedMilliseconds / 1000f <= holdTimerLimit)
        {
            outpost.RequestChangingSpawnTypeServerRpc(NetworkManager.Singleton.LocalClientId);
            stopwatch.Reset();
            stopwatch.Stop();
        }
    }        
}
