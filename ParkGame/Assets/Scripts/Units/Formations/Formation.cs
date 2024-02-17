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
    [SerializeField] private ColorSettings colorSettings;

    public List<GameObject> soldiers = new List<GameObject>();
    public List<GameObject> soldiersSwordmen = new List<GameObject>();
    public List<GameObject> soldiersArchers = new List<GameObject>();
    public List<GameObject> soldiersMolemen = new List<GameObject>();
    public GameObject HorseRoot;
    // box formation
    public GameObject BoxRoot;

    private IFormationType activeFormation;
    private CircleFormation formationCircle;
    private BoxFormation formationBox;
    private Color teamColor;
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
        formationCircle.SetRoots(this, HorseRoot, null);
        formationBox.SetRoots(this, HorseRoot, BoxRoot);
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

    private ISoldier.UnitType AddSoldierByType(GameObject sol) {
        ISoldier.UnitType type = sol.GetComponent<ISoldier>().GetUnitType();
        switch (type) {
            case ISoldier.UnitType.Pawn:
                soldiersSwordmen.Add(sol);
                return ISoldier.UnitType.Pawn;
            case ISoldier.UnitType.Archer:
                soldiersArchers.Add(sol);
                return ISoldier.UnitType.Archer;
            case ISoldier.UnitType.Horseman:
                soldiersMolemen.Add(sol);
                return ISoldier.UnitType.Horseman;
            default:
                break;

        }
        return ISoldier.UnitType.Pawn; // default
    }

    private void SetActiveFormation(FormationType shape) {
        if (shape == FormationType.Circle) { 
            activeFormation = formationCircle; 
        } else if (shape == FormationType.Box) {
            activeFormation = formationBox;
        }

    }

    public void RemoveFromFormation(GameObject soldier, GameObject position, FormationType shape) {
        if (soldiers.Contains(soldier)) {
            soldiers.Remove(soldier);
        }
        SetActiveFormation(shape);
        if (shape == FormationType.Free) { return; }
        activeFormation.RemoveFromFormation(soldier, position);
    }

    // starting point:
    public GameObject GetPositionInFormation(GameObject soldier, FormationType shape) {
        if (soldiers.Contains(soldier)) { return null; } // soldier already there?
        soldiers.Add(soldier);
        ISoldier.UnitType unitType = AddSoldierByType(soldier);

        SetActiveFormation(shape);
        return activeFormation.GetPosition(unitType);
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
