using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerPlacer : MonoBehaviour
{
    [SerializeField]
    AbstractMap _map;

    [SerializeField]
    Vector2[] _locations;

    [SerializeField]
    Transform _pinTransform;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // TODO add touch input
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.position.y;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

            _pinTransform.position = worldPosition;
        }
    }
}
