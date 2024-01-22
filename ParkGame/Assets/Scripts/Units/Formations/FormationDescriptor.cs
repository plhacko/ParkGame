using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationDescriptor : MonoBehaviour {

    public int NumberOfPositions;
    public Vector3 StartingPosition;
    public float Increment;

    public int NumberOfHorsemenPositions;
    public float HorseIncrement;

    void Start() {
        StartingPosition = new Vector3(0.75f, 0.5f, 0);
        Increment = 0.75f;
        HorseIncrement = 2f;
    }

    /* in form: 3 * X + c
    o o o
    o o o
    o o  
    ^= 3 * 2 + 2
    */

    /* startPos + new Vector3(- (m.count % 2) * horseint, -m.count / 2 * horseinc, 0);
     podle stejne logiky:
     m1  o o o  m2
     m3  o o o  m4
     m5  o o o  m6
     m7  o o 
     */

}
