using System;
using UnityEngine;
using System.Collections.Generic;



public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class Sound {
        public string name;
        public AudioClip sound;
    }

    public static AudioManager Instance;

    [SerializeField] private AudioSource sfxSource;

    // formation fanfares: free, circle, box, attack, fallback
    // UI sfx: click on button. start game button (?)
    // notification sfx: {3. 2. 1. start game}, opening VP, got VP, got outpost, lost outpost
    // chimes: won game, lost game
    [Tooltip("{sfxName, clip}")] 
    [SerializeField] private List<Sound> sfxList;

    [SerializeField] private List<AudioClip> pawnAttackSfx_list;
    [SerializeField] private List<AudioClip> archerAttackSfx_list;
    [SerializeField] private List<AudioClip> molemanAttackSfx_list;
    [SerializeField] private List<AudioClip> diedSfx_list;

    private Dictionary<string, AudioClip> sfxDict;
    private bool sfxMute;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        foreach (Sound s in sfxList) {
            sfxDict[s.name] = s.sound;
        }
    }

    public void PlaySFX(AudioClip sfx) {
        sfxSource.PlayOneShot(sfx);
    }

    public void PlaySFX(string sfxName) {
        sfxSource.PlayOneShot(sfxDict[sfxName]);
    }

    private AudioClip GetRandomItem(List<AudioClip> lst) {
        int idx = UnityEngine.Random.Range(0, lst.Count);
        return lst[idx];
    }

    public void PlayPawnAttack() {
        AudioClip sfx = GetRandomItem(pawnAttackSfx_list);
        sfxSource.PlayOneShot(sfx);
    }

    public void PlayArcherAttack() {
        AudioClip sfx = GetRandomItem(archerAttackSfx_list);
        sfxSource.PlayOneShot(sfx);
    }

    public void PlayMolemanAttack() {
        AudioClip sfx = GetRandomItem(molemanAttackSfx_list);
        sfxSource.PlayOneShot(sfx);
    }

    public void PlayDead() {
        AudioClip sfx = GetRandomItem(diedSfx_list);
        sfxSource.PlayOneShot(sfx);
    }

    public void ChangeSfxVolume(float volume) {
        sfxSource.volume = volume;
    }

    public void ToggleSfx() {
        sfxSource.mute = !sfxMute;
    }
}
