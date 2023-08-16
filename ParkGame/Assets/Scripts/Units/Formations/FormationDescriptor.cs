using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationDescriptor : MonoBehaviour {

    public int NumberOfPositions;
    public Vector3 StartingPosition;
    public float Increment;

    void Start() {
       StartingPosition = new Vector3(0.5f, 0.5f, 0);
       Increment = 0.75f;
    }
    /* in form: 3 * X + c
    o o o
    o o o
    o o  
    ^= 3 * 2 + 2
    */
}
