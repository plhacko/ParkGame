using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class BakeNaMesh : MonoBehaviour
{
    private void Start() {
        var NavmeshSurface = FindObjectOfType(typeof(NavMeshSurface));

        Debug.Log("Navmesh? ", NavmeshSurface);
       
    }
}
