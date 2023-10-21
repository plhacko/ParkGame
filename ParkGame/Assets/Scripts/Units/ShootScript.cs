using Units.Archer;
using UnityEngine;
using Unity.Netcode;

public class ShootScript : NetworkBehaviour
{
    [SerializeField] private GameObject ArrowPrefab;

    public void Shoot(Transform target, int damage, bool flipDirection) {
        
        Vector3 p0 = transform.position;
        Vector3 toTarget = (target.position - p0).normalized;
        
        float xOffset = flipDirection ? 0.125f : 0; 
        var arrow = Instantiate(ArrowPrefab, new Vector3(p0.x + xOffset, p0.y - 0.175f, p0.z), Quaternion.Euler(0, 0, Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg));
        arrow.GetComponent<NetworkObject>().Spawn();
        arrow.GetComponent<Arrow>().Initialize(target, damage);
    }
}
