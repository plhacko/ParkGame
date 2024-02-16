using System.Collections.Generic;
using UnityEngine;

public class Formation : MonoBehaviour {
    public enum FormationType { Circle, Box, Free };
    
    public class PositionPair {
        public PositionPair(GameObject i1, bool i2 = false) { gobject = i1; occupated = i2; }

        public GameObject gobject;
        public bool occupated;
    }

    [Tooltip("Show positions for units")]
    [SerializeField] private bool DEBUG;
    [SerializeField] GameObject PositionPrefab;
    [SerializeField] CircleFormation formationCircle;
    [SerializeField] BoxFormation formationBox;
    [SerializeField] private ColorSettings colorSettings;

    public List<GameObject> soldiers = new List<GameObject>();
    public List<GameObject> soldiersSwordmen = new List<GameObject>();
    public List<GameObject> soldiersArchers = new List<GameObject>();
    public List<GameObject> soldiersMolemen = new List<GameObject>();
    Color teamColor;

    public GameObject HorseRoot;
    // box formation
    public GameObject BoxRoot;

    // disable renderer of object
    public void Hide(GameObject go) {
        var sr = go.GetComponent<SpriteRenderer>();
        var mr = go.GetComponent<MeshRenderer>();
        if (sr) { sr.enabled = DEBUG; }
        if (mr) { mr.enabled = DEBUG; }
    }
    
    void UnparentFormation(GameObject go, Vector3 position) {
        go.transform.SetParent(null, true);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
    }

    public void InitializeFormation(int colorIndex) {
        teamColor = colorSettings.Colors[colorIndex].Color;
        Vector3 p = gameObject.transform.position;
        UnparentFormation(BoxRoot, new Vector3(p.x - 2, p.y, p.z));
        formationCircle = GetComponent<CircleFormation>();
        formationBox = GetComponent<BoxFormation>();
        formationCircle.SetRoots(HorseRoot, this);
        formationBox.SetRoots(BoxRoot, HorseRoot, this);
        Hide(BoxRoot, false);
        Hide(HorseRoot, false);
    }

    // disable renderer of object
    private void Hide(GameObject go, bool show) {
        var sr = go.GetComponent<SpriteRenderer>();
        var mr = go.GetComponent<MeshRenderer>();
        sr.color = teamColor;
        if (sr) { sr.enabled = show; }
        if (mr) { mr.enabled = show; }
    }

    // add position mesh for soldier at commander
    // for referencing its location for A*
    public GameObject AddSphere(Vector3 position, float scale, string name, GameObject parent) {
        GameObject sphere = Instantiate(PositionPrefab);
        sphere.name = name;
        sphere.transform.SetParent(parent.transform);
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.transform.localPosition = position;
        Hide(sphere, DEBUG);
        return sphere;
    }

    public void ResetFormation() {
        soldiers.Clear();
        formationCircle.Reset();
        formationBox.Reset();
        soldiersArchers.Clear();
        soldiersSwordmen.Clear();
        soldiersMolemen.Clear();
        RemoveSpheres();
    }

    public void Remove(GameObject soldier, GameObject position, List<GameObject> soldierList, List<GameObject> positionList, bool destroy = true) {
        if (position) { position.GetComponent<PositionDescriptor>().isAssigned = false; }
        soldierList.Remove(soldier);
        if (destroy) {
            positionList.Remove(position);
            Destroy(position);
        }
    }

    private Soldier.UnitType AddSoldierByType(GameObject sol) {
        Soldier.UnitType type = sol.GetComponent<Soldier>().GetUnitType();
        switch (type) {
            case Soldier.UnitType.Pawn:
                soldiersSwordmen.Add(sol);
                return Soldier.UnitType.Pawn;
            case Soldier.UnitType.Archer:
                soldiersArchers.Add(sol);
                return Soldier.UnitType.Archer;
            case Soldier.UnitType.Horseman:
                soldiersMolemen.Add(sol);
                return Soldier.UnitType.Horseman;
            default:
                break;

        }
        return Soldier.UnitType.Pawn; // default
    }

    public void RemoveFromFormation(GameObject soldier, GameObject position, FormationType shape = FormationType.Circle, bool destroy = true) {
        if (soldiers.Contains(soldier)) {
            soldiers.Remove(soldier);
        }
        
        if (shape == FormationType.Circle) { formationCircle.RemoveFromFormation(soldier, position, destroy); }
        if (shape == FormationType.Box) { formationBox.RemoveFromFormation(soldier, position, false); }
    }

    // starting point!
    public GameObject GetPositionInFormation(GameObject soldier, FormationType shape = FormationType.Circle) {
        if (soldiers.Contains(soldier)) { return null; } // soldier already there?
        soldiers.Add(soldier);
        Soldier.UnitType unitType = AddSoldierByType(soldier);
        if (shape == FormationType.Box) {
            return formationBox.GetPosition(unitType);
        }
        // circular formation: archers in smaller circle, swordmen in bigger circle, molemen rotate around
        return formationCircle.GetPosition(unitType);
    }

    private void RemoveSpheres() {
        foreach (Transform t in transform) {
            GameObject child = t.gameObject;
            if (child.name == "CirclePos") {
                Destroy(child);
            }
        }

        foreach (Transform t in BoxRoot.transform) {
            GameObject child = t.gameObject;
            if (child.name == "BoxPos" || child.name == "BoxPosHorse") {
                Destroy(child);
            }
        }

        foreach (Transform t in HorseRoot.transform) {
            GameObject child = t.gameObject;
            if (child.name == "CirclePos") {
                Destroy(child);
            }
        }
    }

    public int GetNumberOfUnassignedPositions(List<GameObject> positions) {
        int count = 0;
        foreach (var p in positions) {
            var pos = p.GetComponent<PositionDescriptor>();
            if (!pos.isAssigned) { count++; }
        }
        return count;
    }

}
