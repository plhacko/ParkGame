using UnityEngine;

public class Revealer : MonoBehaviour
{
    public int Radius = 3;
    
    private void OnDestroy()
    {
        FogOfWar fogOfWar = FindObjectOfType<FogOfWar>();
        if (fogOfWar != null)
        {
            fogOfWar.DeregisterAsRevealer(this);
        }
    }
}
