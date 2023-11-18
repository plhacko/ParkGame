using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float SFXVolume = 1;

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;

    private EventInstance musicInstance;
    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;
    public static AudioManager Instance { get; private set; }

    private void Awake() {
        if (Instance != null) {
            // more than one instance in scene
        }
        Instance = this;

        masterBus = RuntimeManager.GetBus("bus:/");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");

        DontDestroyOnLoad(gameObject);

    }

    public void PlayOneShot(EventReference reference, Vector3 position) {
        RuntimeManager.PlayOneShot(reference, position);
    }

    private void Start() {
        InitializeMusic(FMODEvents.Instance.WowMusic);
    }

    private void InitializeMusic(EventReference musicEventReference) {
        musicInstance = RuntimeManager.CreateInstance(musicEventReference);
        musicInstance.start();
        musicInstance.release();
    }

    private void Update() {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        sfxBus.setVolume(SFXVolume);
    }
}
