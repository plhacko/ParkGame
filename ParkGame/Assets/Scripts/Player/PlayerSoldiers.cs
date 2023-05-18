using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSoldiers : MonoBehaviour
{
    private bool selectedAllSoldiers;
    public List<GameObject> soldiers;

    private void Awake() {
        soldiers = new List<GameObject>();
    }

    void SelectAll() {
        Soldier[] s = FindObjectsOfType<Soldier>();
        Debug.Log("found #" + s.Length.ToString() + " soldiers");
        for (int i = 0; i < s.Length; i++) {
            GameObject sgo = s[i].gameObject;
            s[i].Follow(true);
            soldiers.Add(sgo);
        }
    }

    void DeselectAll() {
        foreach (GameObject sgo in soldiers) {
            sgo.GetComponent<Soldier>().Follow(false);
        }
        soldiers.Clear();
    }

    public bool SwitchMassSelect() {
        selectedAllSoldiers = !selectedAllSoldiers;
        if (selectedAllSoldiers) {
            SelectAll();
        } else {
            DeselectAll();
        }
        return selectedAllSoldiers;
    }


}
