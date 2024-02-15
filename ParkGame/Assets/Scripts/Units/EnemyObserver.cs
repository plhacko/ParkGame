using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class EnemyObserver : MonoBehaviour
{
    [SerializeField] List<Transform> visibleFriends = new();
    [SerializeField] List<Transform> visibleEnemies = new();
    CircleCollider2D CircleCollider2D;
    ITeamMember ParentTeam;
    private void Start()
    {
        CircleCollider2D = GetComponent<CircleCollider2D>();
        ParentTeam = GetComponentInParent<ITeamMember>();
        if (ParentTeam == null) { throw new System.Exception("parent of this object must contain component with interface ITeammember"); }
    }
    public Transform GetClosestEnemy() => GetClosestEnemy(transform);
    public Transform GetClosestEnemy(Transform t)
        => visibleEnemies.Count > 0 ? visibleEnemies.Min(e => (Vector3.Distance(e.position, t.position), e)).e : null;
    public Transform GetFriendEnemy() => GetFriendEnemy(transform);
    public Transform GetFriendEnemy(Transform t)
        => visibleFriends.Count > 0 ? visibleFriends.Min(e => (Vector3.Distance(e.position, t.position), e)).e : null;
    public List<Transform> GetAllEnemies() => visibleEnemies;
    public List<Transform> GetAllFriends() => visibleFriends;
    public Transform GetClosestFriend() => GetClosestFriend(transform);
    public Transform GetClosestFriend(Transform t) 
        => visibleFriends.Count > 0 ? visibleFriends.Min(f => (Vector3.Distance(f.position, t.position), f)).f : null;  

    public void SetRadius(float radius) => CircleCollider2D.radius = radius;

    // for Molerider
    public Transform GetFarthestEnemy() {
        Transform enemy = null;
        float d, dist = -8;
        foreach (Transform e in visibleEnemies) {
            if ((d = Vector3.Distance(e.position, transform.position)) > dist) {
                dist = d;
                enemy = e;
            }
        }
        return enemy;
    }

    public Transform GetRandomEnemy() {
        int randomIndex = Random.Range(0, visibleEnemies.Count);
        return visibleEnemies[randomIndex];
    }

    public Transform GetEnemyInRange(float minDist, float maxDist) {
        foreach (Transform e in visibleEnemies) {
            float dist = Vector3.Distance(e.position, transform.position);
            if (dist >= minDist && dist <= maxDist) {
                return e;
            }
        }
        return null;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<ITeamMember>(out ITeamMember tm))
        {
            if (tm.Team == ParentTeam.Team)
            { visibleFriends.Add(collision.transform); }
            else
            { visibleEnemies.Add(collision.transform); }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<ITeamMember>(out ITeamMember tm))
        {
            visibleFriends.Remove(collision.transform);
            visibleEnemies.Remove(collision.transform);
        }
    }

}
