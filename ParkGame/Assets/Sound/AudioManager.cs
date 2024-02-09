using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;



public class AudioManager : MonoBehaviour
{
    [Serializable]
    public class Sound {
        public string name;
        public AudioClip sound;
    }

    public static AudioManager Instance;

    [SerializeField] public AudioSource sfxSource;
    [SerializeField] public AudioSource clickSfxSource;
    //[SerializeField] private AudioSource notificationsSource;
    public AudioSource notificationsSource;

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
    [SerializeField] private List<AudioClip> soldierClickSfx_list;
    [SerializeField] private List<AudioClip> diedSfx_list;
    [SerializeField] private List<AudioClip> clickSfx_list;

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

    public void MoveSource(AudioSource source, Vector3 position) {
        source.transform.position = position;
    }

    public void PlaySFX(AudioClip sfx) {
        sfxSource.PlayOneShot(sfx);
    }

    public void PlayClickSFX() {
        AudioClip sfx = GetRandomItem(clickSfx_list);
        clickSfxSource.PlayOneShot(sfx);
    }

    public void PlaySFX(string sfxName) {

        sfxSource.PlayOneShot(sfxDict[sfxName]);
    }

    public void PlayNotificationSfx(AudioClip sfx) {
        //notificationsSource.transform.position = playerPosition;
        notificationsSource.PlayOneShot(sfx);
    }

    public static IEnumerator FadeOutFadeIn(AudioSource source, float duration, AudioClip sfx) {
        float currentTime = 0;
        float start = source.volume;
        float prevNotificationVolume = source.volume;
        while (currentTime < duration) {
            currentTime += Time.deltaTime;
            source.volume -= start * Time.deltaTime / duration;
            yield return null;
        }
        source.Stop();
        source.volume = prevNotificationVolume;
        source.PlayOneShot(sfx);
        yield break;
    }

    public void PlayNotificationSFX(string sfxName) {
        if (!notificationsSource) {
            return;
        }
        if (notificationsSource.isPlaying) {
            StartCoroutine(FadeOutFadeIn(notificationsSource, 0.3f, notificationsDict[sfxName]));
            return;
        }
        notificationsSource.PlayOneShot(notificationsDict[sfxName]);
        Debug.Log("playing " + sfxName);
    }

    private AudioClip GetRandomItem(List<AudioClip> lst) {
        int idx = UnityEngine.Random.Range(0, lst.Count);
        return lst[idx];
    }

    public void PlayPawnAttack(Vector3 position) {
        MoveSource(sfxSource, position);
        AudioClip sfx = GetRandomItem(pawnAttackSfx_list);
        sfxSource.PlayOneShot(sfx);
    }

    public void PlayArcherAttack(Vector3 position) {
        MoveSource(sfxSource, position);
        AudioClip sfx = GetRandomItem(archerAttackSfx_list);
        sfxSource.PlayOneShot(sfx);
    }

    public void PlayMolemanAttack(Vector3 position) {
        MoveSource(sfxSource, position);
        AudioClip sfx = GetRandomItem(molemanAttackSfx_list);
        sfxSource.PlayOneShot(sfx);
    }

    public void PlayDead(Vector3 position) {
        MoveSource(sfxSource, position);
        AudioClip sfx = GetRandomItem(diedSfx_list);
        sfxSource.PlayOneShot(sfx);
    }

    public void PlayClickOnDwarf(Vector3 position) {
        MoveSource(sfxSource, position);
        AudioClip sfx = GetRandomItem(soldierClickSfx_list);
        sfxSource.PlayOneShot(sfx);
    }

    public void ChangeSfxVolume(float volume) {
        sfxSource.volume = volume;
    }

    public void ChangeNotificationsVolume(float volume) {
        notificationsSource.volume = volume;
    }

    public void ToggleSfx() {
        sfxMute = !sfxMute;
        sfxSource.mute = sfxMute;
        clickSfxSource.mute = sfxMute;
    }

    public void ToggleNotificationSound() {
        notificationsMute = !notificationsMute;
        notificationsSource.mute = notificationsMute;
    }
}
