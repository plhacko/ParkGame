using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCounter : MonoBehaviour
{
    public string itemName;
    // Start is called before the first frame update
    
    public int maxItems = 3;
    private int currentItems;

    private void Awake()
    {
        currentItems = maxItems;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
