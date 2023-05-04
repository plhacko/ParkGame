using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation : MonoBehaviour
{

    public List<Vector3> soldierPositions = new List<Vector3>();
    private int followingSoldiers;
    private int counter;

    private void Start() 
    {
        counter = 0;
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

    public void ListFormationPositions() 
    {
        soldierPositions.Clear();
        float radius = 2f;
        float alpha = 2 * Mathf.PI / followingSoldiers;
        
        for (int i = 0; i < followingSoldiers; i++) 
        {
            float x = transform.position.x - radius * Mathf.Cos(i * alpha);
            float y = transform.position.y - radius * Mathf.Sin(i * alpha);
            Vector3 vec = new Vector3(x, y, 0);
            soldierPositions.Add(vec);
        }
        
    }

    void Update()
    {
        
    }
}
