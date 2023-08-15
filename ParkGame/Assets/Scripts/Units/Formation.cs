using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation : MonoBehaviour
{
    public class PositionPair {
        public PositionPair(GameObject i1, bool i2 = false) { gobject = i1; occupated = i2; }

        public GameObject gobject;
        public bool occupated;
    }

    public List<Vector3> soldierPositions = new List<Vector3>();
    public List<GameObject> soldiers = new List<GameObject>();
    public List<GameObject> justPositions = new List<GameObject>();
    public List<PositionPair> circlePositions = new List<PositionPair>();
    private int followingSoldiers;
    private int counter;
    //List<Vector3> positions = new List<Vector3>();

    private void Start() 
    {
        counter = 0;
    }

    public enum FormationShape { Circle, Rectangle };

    public void ResetFormation() {
        for (int i = 0; i < soldiers.Count; i++) {
            soldiers.RemoveAt(i);
        }
        //foreach (var pos in justPositions) {
        //    justPositions.Remove(pos);
        //}
        for (int i = 0; i < circlePositions.Count; i++) {
            circlePositions.RemoveAt(i);
        }
        RemoveSpheres();
       
    }

    void FitCircularFormation() {
        // BUT now we added the last sphere, so it should be the same number, I hope!
        var positions = ListCircularPositions();

        if (circlePositions.Count != positions.Count) {
            Debug.Log("NECO NENI V PORADKU S CIRCLE FORMATION!!!" + circlePositions.Count + " " + positions.Count + " " + soldiers.Count);
        } else {
            Debug.Log("stejny pocet pozic v circle formation");
        }
        int j = 0;
        for (int i = 0; i < circlePositions.Count; i++) {
            var a = circlePositions[i].gobject.transform.position;
            var b = positions[j];
            double eps = 0.002f;
            if (Vector3.Distance(a, b) <= eps) {
                // identical, moving on
                j++;
            } else {
                // not identical
                circlePositions[i].gobject.transform.position = b;
                j++;
            }
        }
    }

    // todo!!!!!!!!!!!!!!!!
    public void RemoveFromFormation(GameObject soldier, GameObject position) {
        if (soldiers.Contains(soldier)) {
            soldiers.Remove(soldier);
            foreach (var item in circlePositions) {
                if (item.gobject == position) {
                    item.occupated = false;
                    Destroy(position); 
                    item.gobject = null;
                    circlePositions.Remove(item);
                    // translate existing positions to fit the circular formation again
                    FitCircularFormation();
                    return;
                }
            }
        }
        return;
    }


    public GameObject GetFormation(GameObject soldier, FormationShape shape = Formation.FormationShape.Circle) {
        if (soldiers.Contains(soldier)) { return null; } // soldier already there?

        soldiers.Add(soldier); 
        ListFormationPositions();
        return GetPosition();
    }

    // add parameter for formation type
    public GameObject GetPosition(FormationShape shape = FormationShape.Circle) {
        // var positionList;...
        // if (shape == FormationShape.Circle) { positionList = circlePositions;}
        // else if (shape == FormationShape.Rectangle) { positionList = rectPositions;} 
        // ...

        Debug.Log("num of positions" + circlePositions.Count);
        foreach (var pos in circlePositions) { // in positionList
            if (pos.occupated) { continue; }
            pos.occupated = true;
            Debug.Log("assigning position" + pos.gobject.transform.position);
            return pos.gobject;
        }
        Debug.Log("no free position!");
        return null;
    }
        /*
         * compute positions: when commander is staying still; if new follower added 
         */
    public void addFollower() 
    {
        Debug.Log("add follower");
        followingSoldiers++;
    }

    public void removeFollower() 
    {
        Debug.Log("remove follower");
        followingSoldiers--;
        if (followingSoldiers < 0) 
        {
            followingSoldiers = 0;
        }
    }

    
    public Vector3 GetPositionInFormation() {
        if (soldierPositions.Count == 0) {
            ListFormationPositions();
        }
        if (counter >= soldierPositions.Count) {
            counter = 0;
        }
        var position = soldierPositions[counter];
        counter++;
        Debug.Log("position " + position);
        return position;
    }
    

    private void RemoveSpheres() {
        foreach (Transform t in transform) {
            GameObject child = t.gameObject;
            if (child.name == "CirclePos") {
                Destroy(child);
            }
        } 
    }

    public List<Vector3> ListCircularPositions() {
        float radius = 1f;// 2f; // radius from commander
        float alpha = 2 * Mathf.PI / soldiers.Count;

        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < soldiers.Count; i++) {
            float x = transform.position.x - radius * Mathf.Cos(i * alpha);
            float y = transform.position.y - radius * Mathf.Sin(i * alpha);
            Vector3 vec = new Vector3(x, y, 0);
            positions.Add(vec);

        }
        return positions;
    }

    void Add1PositionToCircularFormation(Vector3 pos) {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;
        sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        sphere.transform.SetParent(transform);
        sphere.name = "CirclePos";
        circlePositions.Add(new PositionPair(sphere));
    }

    public void ListFormationPositions() 
    {
        //soldierPositions.Clear();

        //RemoveSpheres();

        // draw positions for the number of soldiers
        var positions = ListCircularPositions();

        // add sphere on the last position of the recomputed circle formation
        if (circlePositions.Count < positions.Count) {
            Add1PositionToCircularFormation(positions[positions.Count - 1]);
            /*
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = positions[positions.Count - 1];
            sphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            sphere.transform.SetParent(transform);
            sphere.name = "CirclePos";
            circlePositions.Add(new PositionPair(sphere));
            */
            //    justPositions.Add(sphere);
        }
        // just one for loop for positions???
        // what is bigger number?
        // let's say that we are just adding soldiers -> positions.Count > circlePositions.Count
        // BUT now we added the last sphere, so it should be the same number, I hope!
        if (circlePositions.Count != positions.Count) {
            Debug.Log("NECO NENI V PORADKU S CIRCLE FORMATION!!!" + circlePositions.Count + " " + positions.Count + " " + soldiers.Count);
        } else {
            Debug.Log("stejny pocet pozic v circle formation");
        }

        FitCircularFormation();
        /*
        double eps = 0.002f;
        int j = 0;
        for (int i = 0; i < circlePositions.Count; i++) {
            var a = circlePositions[i].gobject.transform.position;
            var b = positions[j];
            if (Vector3.Distance(a, b) <= eps) {
                // identical, moving on
                j++;
            } else {
                // not identical
                circlePositions[i].gobject.transform.position = b;
                j++;
            }
        */
        }
    }

