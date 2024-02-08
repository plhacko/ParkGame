using UnityEngine;

public class AssignNotificationSource : MonoBehaviour
{
    void Start()
    {
        AudioManager.Instance.notificationsSource = GetComponent<AudioSource>();
    }
}
