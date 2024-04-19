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

    [Serializable]
    public class PoolItem {
        public Vector3 position;
        public AudioClip sound;
    }

    public static AudioManager Instance;

    [SerializeField] private AudioSource clickSfxSource;
    public AudioSource commandsSource;
    public AudioSource notificationsSource;

    // formation fanfares: free, circle, box, attack, fallback
    // UI sfx: click on button. start game button (?)
    // notification sfx: {3. 2. 1. start game}, opening VP, got VP, got outpost, lost outpost
    // chimes: won game, lost game
    [Tooltip("{sfxName, clip}")] 
    [SerializeField] private List<Sound> notificationsList;

    [SerializeField] private List<AudioClip> pawnAttackSfx_list;
    [SerializeField] private List<AudioClip> archerAttackSfx_list;
    [SerializeField] private List<AudioClip> molemanAttackSfx_list;
    [SerializeField] private List<AudioClip> soldierClickSfx_list;
    [SerializeField] private List<AudioClip> diedSfx_list;
    [SerializeField] private List<AudioClip> arrowAttackSfx_list;
    [SerializeField] private List<AudioClip> clickSfx_list;

    private Dictionary<string, AudioClip> notificationsDict = new Dictionary<string, AudioClip>();
    private AudioPool pool;
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

    private void OnEnable() {
        // list to dictionary
        foreach (Sound s in notificationsList) {
            notificationsDict[s.name] = s.sound;
        }
        pool = gameObject.GetComponentInChildren<AudioPool>();
        ResetSoundSettings();
    }

    // after leaving the game -> unmute everything
    public void ResetSoundSettings() {
        clickSfxSource.mute = false;
        if (notificationsSource) { notificationsSource.mute = false; }
        if (commandsSource) { commandsSource.mute = false; }
        pool.ToggleSfx(false);
    }

    private void PoolSfxBasedOnCommander(AudioClip sfx, Vector3 position) {
        position.z = notificationsSource.transform.position.z; // move to the same level as commander
        float distance = Vector3.Distance(position, notificationsSource.transform.position);
        if (distance <= 10) {
            pool.PlayAtPoint(sfx, position);
        }
    }


    public void PlayClickSFX() {
        // has its own audio source
        AudioClip sfx = GetRandomItem(clickSfx_list);
        clickSfxSource.PlayOneShot(sfx);
    }
    public void PlayNotificationSfx(AudioClip sfx) {
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

    public void PlayCommandSFX(string sfxName) {
        if (!commandsSource) {
            return;
        }
        if (commandsSource.isPlaying) {
            StartCoroutine(FadeOutFadeIn(commandsSource, 0.3f, notificationsDict[sfxName]));
            return;
        }
        commandsSource.PlayOneShot(notificationsDict[sfxName]);
        Debug.Log("playing command: " + sfxName);
    }

    public void PlayNotificationSFX(string sfxName) {
        if (!notificationsSource) {
            return;
        }
        notificationsSource.PlayOneShot(notificationsDict[sfxName]);
        Debug.Log("playing notification: " + sfxName);
    }

    private AudioClip GetRandomItem(List<AudioClip> lst) {
        int idx = UnityEngine.Random.Range(0, lst.Count);
        return lst[idx];
    }

    public void PlayPawnAttack(Vector3 position) {
        AudioClip sfx = GetRandomItem(pawnAttackSfx_list);
        PoolSfxBasedOnCommander(sfx, position);
    }

    public void PlayArcherAttack(Vector3 position) {
        AudioClip sfx = GetRandomItem(archerAttackSfx_list);
        PoolSfxBasedOnCommander(sfx, position);
    }

    public void PlayArrowAttack(Vector3 position) {
        AudioClip sfx = GetRandomItem(arrowAttackSfx_list);
        PoolSfxBasedOnCommander(sfx, position);
    }

    public void PlayMolemanAttack(Vector3 position) {
        AudioClip sfx = GetRandomItem(molemanAttackSfx_list);
        PoolSfxBasedOnCommander(sfx, position);
    }

    public void PlayDead(Vector3 position) {
        AudioClip sfx = GetRandomItem(diedSfx_list);
        PoolSfxBasedOnCommander(sfx, position);
    }

    public void PlayClickOnDwarf(Vector3 position) {
        AudioClip sfx = GetRandomItem(soldierClickSfx_list);
        PoolSfxBasedOnCommander(sfx, position);
    }

    public void ChangeSfxVolume(float volume) {
        clickSfxSource.volume = volume;
        pool.ChangeSfxVolume(volume);
    }

    public void ChangeNotificationsVolume(float volume) {
        notificationsSource.volume = volume;
        commandsSource.volume = volume;
    }

    public void ToggleSfx() {
        sfxMute = !sfxMute;
        clickSfxSource.mute = sfxMute;

        pool.ToggleSfx(sfxMute);
    }

    public void ToggleNotificationSound() {
        notificationsMute = !notificationsMute;
        notificationsSource.mute = notificationsMute;
        commandsSource.mute = notificationsMute;
    }
}
