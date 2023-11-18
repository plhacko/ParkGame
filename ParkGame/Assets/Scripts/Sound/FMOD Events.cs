using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;    

public class FMODEvents : MonoBehaviour
{
    [field: SerializeField] public EventReference SwordHitSFX { get; private set; }
    [field: SerializeField] public EventReference ArrowHitSFX { get; private set; }
    [field: SerializeField] public EventReference WowMusic { get; private set; }
    public static FMODEvents Instance { get; private set; }
    private void Awake() {
        if (Instance != null) {
            // not a singleton!
        }
        Instance = this;
    }
}
