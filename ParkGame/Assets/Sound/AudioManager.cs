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
    [SerializeField] private AudioSource notificationsSource;

    // formation fanfares: free, circle, box, attack, fallback
    // UI sfx: click on button. start game button (?)
    // notification sfx: {3. 2. 1. start game}, opening VP, got VP, got outpost, lost outpost
    // chimes: won game, lost game
    [Tooltip("{sfxName, clip}")] 
    [SerializeField] private List<Sound> sfxList;
    [SerializeField] private List<Sound> notificationsList;

    [SerializeField] private List<AudioClip> pawnAttackSfx_list;
    [SerializeField] private List<AudioClip> archerAttackSfx_list;
    [SerializeField] private List<AudioClip> molemanAttackSfx_list;
    [SerializeField] private List<AudioClip> diedSfx_list;

    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> notificationsDict = new Dictionary<string, AudioClip>();
    
    private bool sfxMute;
    private bool notificationsMute;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start() {
        // list to dictionary
        foreach (Sound s in sfxList) {
            sfxDict[s.name] = s.sound;
        }
        foreach (Sound s in notificationsList) {
            notificationsDict[s.name] = s.sound;
        }
    }

    public void PlaySFX(AudioClip sfx) {
        sfxSource.PlayOneShot(sfx);
    }

    public void PlaySFX(string sfxName) {

        sfxSource.PlayOneShot(sfxDict[sfxName]);
    }

    public void PlayNotificationSfx(AudioClip sfx) {
        notificationsSource.PlayOneShot(sfx);
    }

    public void PlayNotificationSFX(string sfxName) {
        notificationsSource.PlayOneShot(notificationsDict[sfxName]);
        Debug.Log("playing " + sfxName);
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

    public void ChangeNotificationsVolume(float volume) {
        notificationsSource.volume = volume;
    }

    public void ToggleSfx() {
        sfxSource.mute = !sfxMute;
    }

    public void ToggleNotificationSound() {
        notificationsSource.mute = !notificationsMute;
    }
}
