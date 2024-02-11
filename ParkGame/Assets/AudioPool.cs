using UnityEngine;
using System.Collections.Generic;
public class AudioPool : MonoBehaviour {
    [SerializeField] private List<AudioSource> audioSources = new List<AudioSource>(); // all sources: busy + free
    private List<AudioSource> audioSourcesFree = new List<AudioSource>(); // all free sources
    private int lastCheckFrame = -1;

    private void Start() {
        audioSourcesFree.Clear();
        foreach (var s in audioSources) {
            audioSourcesFree.Add(s);
        }
    }

    private void CheckInUse() {
        foreach (AudioSource source in audioSources) {
            if (!source.isPlaying && !audioSourcesFree.Contains(source)) {
                audioSourcesFree.Add(source);
            }
        }
    }

    private void PlayOnFreeSource(AudioClip sfx, Vector3 position) {
        AudioSource source = audioSourcesFree[audioSourcesFree.Count - 1]; // last free source
        audioSourcesFree.Remove(source);
        source.transform.position = position;
        source.clip = sfx;
        source.Play();
    }

    public void PlayAtPoint(AudioClip sfx, Vector3 position) {
        if (lastCheckFrame != Time.frameCount) {
            lastCheckFrame = Time.frameCount;
            CheckInUse();
        }

        if (audioSourcesFree.Count > 0) {
            PlayOnFreeSource(sfx, position);
        }
    }

    public void ToggleSfx(bool mute) {
        foreach (AudioSource s in audioSources) {
            s.mute = mute;
        }
    }

    public void ChangeSfxVolume(float volume) {
        foreach (AudioSource s in audioSources) {
            s.volume = volume;
        }
    }
}