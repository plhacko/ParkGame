using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Player;

public class Formation : MonoBehaviour {
    public enum FormationType { Circle, Box, Free };
    
    public class PositionPair {
        public PositionPair(GameObject i1, bool i2 = false) { gobject = i1; occupated = i2; }

        public GameObject gobject;
        public bool occupated;
    }

    public bool DEBUG;
    [SerializeField] GameObject PositionPrefab;
    public List<GameObject> soldiers = new List<GameObject>();
    
    public List<GameObject> soldiersSwordmen = new List<GameObject>();
    public List<GameObject> soldiersArchers = new List<GameObject>();
    public List<GameObject> soldiersMolemen = new List<GameObject>();

    private int team => playerController.Team;

    // circle formation - positions for soldiers
    public List<GameObject> FormationCircle = new List<GameObject>();
    
    public List<GameObject> FormationCircleInner = new List<GameObject>();
    public List<GameObject> FormationCircleOuter = new List<GameObject>();
    public List<GameObject> FormationCircleForHorses = new List<GameObject>();
    public GameObject HorseRoot;

    // box formation
    public GameObject BoxRoot;
    public List<GameObject> FormationBox = new List<GameObject>(); // are in hierarchy under BoxRoot
    public List<GameObject> FormationBoxForHorses = new List<GameObject>(); // are in hierarchy under BoxRoot

    private PlayerController playerController;

    private void Awake() {
        playerController = GetComponent<PlayerController>();
        DEBUG = true;
    }

    // disable renderer of object
    public void Hide(GameObject go) {
        var sr = go.GetComponent<SpriteRenderer>();
        var mr = go.GetComponent<MeshRenderer>();
        if (sr) { sr.enabled = DEBUG; }
        if (mr) { mr.enabled = DEBUG; }
    }

    private void Update() {
        RotateBoxFormation();
        RotateHorseFormation();

        if (Input.GetKeyDown(KeyCode.Space)) {
            Add1PositionToBoxFormation();
        }
    }
    
    void UnparentFormation(GameObject go, Vector3 position) {
        go.transform.SetParent(null, true);
        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        Hide(BoxRoot, false);
    }

    public void InitializeFormation() {
        Vector3 p = gameObject.transform.position;
        UnparentFormation(BoxRoot, new Vector3(p.x - 2, p.y, p.z));
    }

    // disable renderer of object
    public void Hide(GameObject go, bool show) {
        var sr = go.GetComponent<SpriteRenderer>();
        var mr = go.GetComponent<MeshRenderer>();
        if (sr) { sr.enabled = show; }
        if (mr) { mr.enabled = show; }
    }

    // add position mesh for soldier at commander
    // for referencing its location for A*
    GameObject AddSphere(Vector3 position, float scale, string name, GameObject parent) {
        GameObject sphere = Instantiate(PositionPrefab);
        sphere.name = name;
        sphere.transform.SetParent(parent.transform);
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.transform.localPosition = position;
        Hide(sphere, DEBUG);
        return sphere;
    }

    void Add1PositionToCircularFormation(Vector3 pos, List<GameObject> lst, GameObject parent) {
        var sphere = AddSphere(pos, 0.2f, "CirclePos", parent);
        
        lst.Add(sphere);
    }

    // temp
    void CreateSpiral() {
        int numSol = 12;//4;//36;
        float coils = 1.13f * Mathf.Sqrt(24);//3; // 1.4*sqrt(numSol) pro 4 pekny oblouk
        Debug.Log("coils " + coils);
        // value of theta corresponding to end of last coil
        float thetaMax = Mathf.PI * 2 * coils;
        
        int sphSpir = 0;
        float radius = (numSol < 6 ? 1 : 3.6f);
        Debug.Log("radius pro 50 " + radius + " pro 3 " + 0.12 * 3 + " pro 12 " + 0.12 * 12);
        // How far to step away from center for each side.
        float awayStep = radius / thetaMax;
        // distance between points to plot
        float chord = 0.65f;
        // For every side, step around and away from center.
        // start at the angle corresponding to a distance of chord
        // away from centre.
        Debug.Log("for loop" + " " + chord / awayStep + " "+ thetaMax);
        for (float theta = chord / awayStep; theta <= thetaMax;) {
            if (sphSpir >= numSol) {
                return;
            }
            sphSpir++;
            // How far away from center
            float away = awayStep * theta;
            // How far around the center.
            float around = theta + 0.5f;
            // Convert 'around' and 'away' to X and Y.
            float x = 0 + Mathf.Cos(around) * away;
            float y = 0 + Mathf.Sin(around) * away;
            // Now that you know it, do it.
            var sphere = AddSphere(new Vector3(x, y, 0), 0.2f, "SpiralPos", this.gameObject);
            // to a first approximation, the points are on a circle
            // so the angle between them is chord/radius
            theta += chord / away;
        }
    }

    void Add1PositionToBoxFormation() {
        var formDescr = BoxRoot.GetComponent<FormationDescriptor>();
        int c = formDescr.NumberOfPositions;
        float inc = formDescr.Increment;
        var pos = formDescr.StartingPosition;
        Vector3 position = pos + new Vector3(-(c % 3) * inc, - c/3 * inc, 0);

        var sphere = AddSphere(position, 0.2f, "BoxPos", BoxRoot);
        FormationBox.Add(sphere);
        formDescr.NumberOfPositions++;
    }

    void Add1PositionOnTheSide() {
        var formDescr = BoxRoot.GetComponent<FormationDescriptor>(); // 
        int c = formDescr.NumberOfHorsemenPositions; // add definition to the horse places? how to deal with moles???
        float horseinc = formDescr.HorseIncrement;
        var pos = formDescr.StartingPosition;
        float x = c % 2 == 1 ? -1 : 1;
        Vector3 position = new Vector3(x * horseinc, -c / 2 * horseinc + pos.y, 0);

        var sphere = AddSphere(position, 0.2f, "BoxPosHorse", BoxRoot);
        FormationBoxForHorses.Add(sphere);
        formDescr.NumberOfHorsemenPositions++;
    }

    void RotateHorseFormation() {
        HorseRoot.transform.Rotate(0, 0, 0.5f);    
    }

    // rotate the box formation according to the commander's direction of movement
    void RotateBoxFormation() {
        if (!BoxRoot) { return; }
        Vector3 from = BoxRoot.transform.up;
        Vector3 to = transform.position - BoxRoot.transform.position;

        float angle = Vector3.SignedAngle(from, to, BoxRoot.transform.forward);
        float distance = Vector3.Distance(transform.position, BoxRoot.transform.position);
        if (Mathf.Abs(angle) > 10 && distance > 0.9) { // otherwise don't rotate
            BoxRoot.transform.Rotate(0.0f, 0.0f, angle);
        }
        if (distance > 1) {
            Vector3 direction = (transform.position - BoxRoot.transform.position);
            BoxRoot.transform.position += direction * Time.deltaTime * 0.6f;
        }
    }

    public void ResetFormation() {
        soldiers.Clear();
        soldiersArchers.Clear();
        soldiersSwordmen.Clear();
        soldiersMolemen.Clear();

        FormationCircle.Clear();
        FormationCircleInner.Clear();
        FormationCircleOuter.Clear();
        FormationCircleForHorses.Clear();

        FormationBox.Clear();
        FormationBoxForHorses.Clear();
        if (BoxRoot) {
            BoxRoot.GetComponent<FormationDescriptor>().NumberOfPositions = 0;
            BoxRoot.GetComponent<FormationDescriptor>().NumberOfHorsemenPositions = 0;
        }
        RemoveSpheres();
       
    }

    public void RemoveFromFormation(GameObject soldier, GameObject position, FormationType shape = FormationType.Circle) {
        Soldier.UnitType unitType = soldier.GetComponent<Soldier>().GetUnitType();
        var positionList = FormationCircle; // now not used list
        if (shape == FormationType.Box) {
            if (unitType == Soldier.UnitType.Horseman) {
                positionList = FormationBoxForHorses;
            } else {
                positionList = FormationBox;
            }
        } else {
            if (unitType == Soldier.UnitType.Pawn) {
                positionList = FormationCircleOuter;
            } else if (unitType == Soldier.UnitType.Archer) {
                positionList = FormationCircleInner;
            } else {
                positionList = FormationCircleForHorses;
            }
        }


        if (soldiers.Contains(soldier)) {
            soldiers.Remove(soldier);
            foreach (var item in positionList) {
                var pos = item.GetComponent<PositionDescriptor>();
                if (item == position) {
                    pos.isAssigned = false;
                    positionList.Remove(item);
                    Destroy(position);
                    return;
                }
            }
        }
        
    }

    Soldier.UnitType AddSoldierByType(GameObject sol) {
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

    public GameObject GetPositionInFormation(GameObject soldier, FormationType shape = FormationType.Circle) {
        if (soldiers.Contains(soldier)) { return null; } // soldier already there?

        soldiers.Add(soldier);
        Soldier.UnitType unitType = AddSoldierByType(soldier);

        if (shape == FormationType.Box) { 
            return GetPositionInBox(unitType); 
        }
        // circular formation: archers in smaller circle, swordmen in bigger circle
        return GetPositionInCircle(unitType);
    }

    // add parameter for formation type
    public GameObject GetPositionInBox(Soldier.UnitType unitType) {
        var positionList = FormationBox;
        //if (FormationBox.Count < soldiers.Count) {
        if (unitType != Soldier.UnitType.Horseman && FormationBox.Count < soldiersArchers.Count + soldiersSwordmen.Count) {
            Add1PositionToBoxFormation();
        } 
        if (unitType == Soldier.UnitType.Horseman) {
            Add1PositionOnTheSide();
            positionList = FormationBoxForHorses;
        }
       

        foreach (var go in positionList) { 
            var pos = go.GetComponent<PositionDescriptor>();
            if (pos.isAssigned) { continue; }
            pos.isAssigned = true;
            return go;
        }
        return null;
    }

    // add one position to circle formation: inner or outer circle
    public GameObject GetPositionInCircle(Soldier.UnitType unitType) {        
        var positionList = FormationCircleOuter;
        var soldierList = soldiersSwordmen;
        GameObject parent = gameObject;

        if (unitType == Soldier.UnitType.Archer) {
            positionList = FormationCircleInner;
            soldierList = soldiersArchers;
        } else if (unitType == Soldier.UnitType.Horseman) {
            positionList = FormationCircleForHorses;
            soldierList = soldiersMolemen;
            parent = HorseRoot;
        }

        var positions = ListCircularPositionsByUnitType(unitType);

        if (soldierList.Count > positionList.Count) {
            Add1PositionToCircularFormation(positions[positions.Count - 1], positionList, parent);
        }

        AdjustFormation(positionList, positions);

        foreach (var go in positionList) {
            var pos = go.GetComponent<PositionDescriptor>();
            if (pos.isAssigned) { continue; }
            pos.isAssigned = true;
            return go;
        }
        return null;

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

    List<GameObject> AdjustFormation(List<GameObject> formationList, List<Vector3> positions) {
        int j = 0;
        //Debug.Log("adjust formation!!!!!");
        for (int i = 0; i < formationList.Count; i++) {
           // var a = formationList[i].transform.localPosition;
            var a = formationList[i].transform.position;
            var b = positions[j];
            double eps = 0.002f;
            if (Vector3.Distance(a, b) <= eps) {
                // identical, moving on
                j++;
            } else {
                // not identical
                formationList[i].transform.position = b;
                j++;
            }
        }
        return formationList;
    }

    public List<Vector3> ListBoxPositions() {
        if (soldiers.Count < 1) { return null; }

        List<Vector3> positions = new List<Vector3>();
        var formDescr = BoxRoot.GetComponent<FormationDescriptor>();
        float inc = formDescr.Increment;
        var sp = formDescr.StartingPosition;
        int c = formDescr.NumberOfPositions;

        for (int i = 0; i < c; i++) {
            Vector3 pos = sp + new Vector3(-(i % 3) * inc, -i / 3 * inc, 0);
            pos += BoxRoot.transform.position;

            Vector3 from = BoxRoot.transform.up;
            Vector3 to = transform.position - BoxRoot.transform.position;
            float angle = Vector3.SignedAngle(from, to, BoxRoot.transform.forward);
            var tmp = new GameObject();
            tmp.transform.position = pos;
            tmp.transform.Rotate(0,0,angle);
            pos = tmp.transform.position;
            Destroy(tmp);
            positions.Add(pos);
        }
        return positions;

    }

    public List<Vector3> ListCircularPositions() {

        if (soldiers.Count < 1) { return null; }
        float radius = 1f; // radius from commander
        float alpha = 2 * Mathf.PI / soldiers.Count;

        int counter = 0;

        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < soldiers.Count; i++) {
            float x = transform.position.x - radius * Mathf.Cos(i * alpha);
            float y = transform.position.y - radius * Mathf.Sin(i * alpha);
            Vector3 vec = new Vector3(x, y, 0);
            positions.Add(vec);
            counter++;
        }


        return positions;
    }

    public List<Vector3> ListCircularPositionsByUnitType(Soldier.UnitType unitType) {
        var listOfSoldiers = soldiersSwordmen;
        float radius = 1.6f;
        float j = 0;
        if (unitType == Soldier.UnitType.Archer) {
            listOfSoldiers = soldiersArchers;
            radius = 0.8f;
            j = 0.5f;
        } else if (unitType == Soldier.UnitType.Horseman) {
            listOfSoldiers = soldiersMolemen;
            radius = 2.5f;
        }

        if (listOfSoldiers.Count < 1) { return null; }
        
        float alpha = 2 * Mathf.PI / listOfSoldiers.Count;

        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < listOfSoldiers.Count; i++) {
            float x = transform.position.x - radius * Mathf.Cos(i * alpha + j);
            float y = transform.position.y - radius * Mathf.Sin(i * alpha + j);
            Vector3 vec = new Vector3(x, y, 0);
            positions.Add(vec);
        }

        return positions;
    }

}

