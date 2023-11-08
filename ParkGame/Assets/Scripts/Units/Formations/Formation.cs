using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Player;

public class Formation : NetworkBehaviour {
//public class Formation : MonoBehaviour {
    public enum FormationType { Circle, Box, Free };

    // tmp counter
    float ccc;

    public class PositionPair {
        public PositionPair(GameObject i1, bool i2 = false) { gobject = i1; occupated = i2; }

        public GameObject gobject;
        public bool occupated;
    }

    [SerializeField] GameObject PositionPrefab;
    public List<GameObject> soldiers = new List<GameObject>();
    
    public List<GameObject> soldiersSwordmen = new List<GameObject>();
    public List<GameObject> soldiersArchers = new List<GameObject>();

    private int Team;

    // circle formation - positions for soldiers
    public List<GameObject> FormationCircle = new List<GameObject>();
    
    // box formation
    public GameObject BoxRoot;
    public List<GameObject> FormationBox = new List<GameObject>(); // are in hierarchy under BoxRoot

    [ClientRpc]
    void UnparentFormationClientRpc() {
        var p = gameObject.transform.position;
        BoxRoot.transform.SetParent(null, true);
        BoxRoot.transform.position = new Vector3(p.x - 2, p.y, p.z);
        BoxRoot.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
        Hide(BoxRoot);
    }

    public void StartFormation() {
        UnparentFormationClientRpc();
        Team = gameObject.GetComponent<PlayerController>().Team;
    }

    // disable renderer of object
    public void Hide(GameObject go, bool hide=true) {
        var sr = go.GetComponent<SpriteRenderer>();
        var mr = go.GetComponent<MeshRenderer>();
        if (sr) { sr.enabled = !hide; }
        if (mr) { mr.enabled = !hide; }
    }

    private void Update() {
        RotateBoxFormation();

        if (ccc > 10) {
            // test
            //CallToMyTeammates();
            ccc = 0;
        } else { 
            ccc += Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Add1PositionToBoxFormation();
        }
    }

    // add position mesh for soldier at commander
    // for referencing its location for A*
    GameObject AddSphere(Vector3 position, float scale, string name, GameObject parent) {
        GameObject sphere = Instantiate(PositionPrefab);
        sphere.name = name;
        sphere.transform.SetParent(parent.transform);
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.transform.localPosition = position;
        //Hide(sphere);
        return sphere;
    }

    void CallToMyTeammates() {
        var soldiers = FindObjectsOfType<Soldier>();
        List<GameObject> myTeamSoldiers = new List<GameObject>();
        foreach (var s in soldiers) {
            if (s.Team == Team) {
                myTeamSoldiers.Add(s.gameObject);
                if (s.GetCommanderWhomIFollow() == gameObject.transform) {
                } else {
                }
            }
        }

    }

    void Add1PositionToCircularFormation(Vector3 pos) {
        var sphere = AddSphere(pos, 0.2f, "CirclePos", gameObject);
        FormationCircle.Add(sphere);
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
        if (!BoxRoot) {
            //StartFormation();
        }

        soldiers.Clear();
        soldiersArchers.Clear();
        soldiersSwordmen.Clear();

        FormationCircle.Clear();
        FormationBox.Clear();
        if (BoxRoot) {
            BoxRoot.GetComponent<FormationDescriptor>().NumberOfPositions = 0;
        }
        RemoveSpheres();
       
    }

    public void RemoveFromFormation(GameObject soldier, GameObject position, FormationType shape = FormationType.Circle) {
        var positionList = FormationCircle;
        if (shape == FormationType.Box) { positionList = FormationBox; }
        
        if (soldiers.Contains(soldier)) {
            soldiers.Remove(soldier);
            foreach (var item in positionList) {
                var pos = item.GetComponent<PositionDescriptor>();
                if (item == position) {
                    pos.isAssigned = false;
                    positionList.Remove(item);
                    Destroy(position);
                    FitFormation(shape);
                    return;
                }
            }
        }
    }

    void AddSoldierByType(GameObject sol) {
        Soldier.UnitType type = sol.GetComponent<Soldier>().GetUnitType();
        switch (type) {
            case Soldier.UnitType.Pawn:
                soldiersSwordmen.Add(sol);
                break;
            case Soldier.UnitType.Archer:
                soldiersArchers.Add(sol);
                break;
            
            default:
                break;
            
        }
    }

    public GameObject GetPositionInFormation(GameObject soldier, FormationType shape = FormationType.Circle) {
        if (soldiers.Contains(soldier)) { return null; } // soldier already there?

        soldiers.Add(soldier);

        AddSoldierByType(soldier);

        ListFormationPositions(shape);
        return GetPosition(shape);
    }

    // add parameter for formation type
    public GameObject GetPosition(FormationType shape = FormationType.Circle) {
        var positionList = FormationCircle;
        if (shape == FormationType.Box) { positionList = FormationBox; } 
        
        foreach (var go in positionList) { 
            var pos = go.GetComponent<PositionDescriptor>();
            if (pos.isAssigned) { continue; }
            pos.isAssigned = true;
            return go;
        }
        Debug.Log("no free position!");
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
            if (child.name == "BoxPos") {
                Destroy(child);
            }
        }
    }

    void FitFormation(FormationType shape) {
        switch (shape) {
            case FormationType.Circle:
                FitCircularFormation();
                break;
            case FormationType.Box:
                FitBoxFormation();
                break;
            default:
                break;
        }
    }

    void AdjustFormation(List<GameObject> formationList, List<Vector3> positions) {
        int j = 0;
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
    }

    void FitBoxFormation() {
        // fill in gaps by reasigning soldiers
        var positions = ListBoxPositions();
        //AdjustFormation(FormationBox, positions);   
    }

    void FitCircularFormation() {
        var positions = ListCircularPositions();
        AdjustFormation(FormationCircle, positions);
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
        float inc = 0.1f;

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

    // list positions and add new gameobject for the position if needed
    // then adjust the position objects (now only for circle)
    public void ListFormationPositions(FormationType shape = FormationType.Circle) {
        // draw positions for following soldiers
        if (shape == FormationType.Circle) {
            var positions = ListCircularPositions();

            // add sphere on the last position of the recomputed circle formation
            if (FormationCircle.Count < positions.Count) {
                Add1PositionToCircularFormation(positions[positions.Count - 1]);
            }

            // recount positions, ajdust the number of followers changed
            FitCircularFormation();
        }
        if (shape == FormationType.Box) {
            if (FormationBox.Count < soldiers.Count) {
                Add1PositionToBoxFormation();
            }
            FitBoxFormation();
        }
    }

    }

