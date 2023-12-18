using UnityEngine;

public class PlaySoundTest : MonoBehaviour
{
    [SerializeField]
    private AudioClip sfx1;
    [SerializeField]
    private AudioClip sfx2;
    [SerializeField]
    private AudioClip sfx3;

    public float sfx1time;
    public float sfx2time;
    public float sfx3time;
    public float counter1time;
    public float counter2time;
    public float counter3time;

    private void Start() {
        AudioManager.Instance.PlaySFX(sfx3);
    }

    float UpdateAndPlay(AudioClip sfx, float time, float counter) {
        if (counter <= 0) {
            counter = time;
            AudioManager.Instance.PlaySFX(sfx);
        } else {
            counter -= Time.deltaTime;
        }
        return counter;
    }

    void Update()
    {
        counter1time = UpdateAndPlay(sfx1, sfx1time, counter1time);
        counter2time = UpdateAndPlay(sfx2, sfx2time, counter2time);
        counter3time = UpdateAndPlay(sfx3, sfx3time, counter3time);
    }
}
