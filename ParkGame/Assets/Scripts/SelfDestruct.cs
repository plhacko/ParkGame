using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [SerializeField] float TimeToLive = 4;
    public float TimeLived = 0;
    void Update()
    {
        TimeLived += Time.deltaTime;
        if (TimeLived >= TimeToLive) {
            Destroy(gameObject);
        }
    }
}
