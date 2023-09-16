using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class ShootScript : NetworkBehaviour
{
    [SerializeField] GameObject ArrowPrefab;
    public Transform Target;
    public GameObject Arrow;
    [SerializeField] float TimeTillSelfdestruct;
    public float ArrowTime;
    private int Damage;
    public Vector3 TargetPosition;

    private void Start() {
        DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(200, 10); // somewhere else?
    }

    private void DrawArrow() {
        Vector3 p0 = transform.position;
        Arrow = Instantiate(ArrowPrefab, new Vector3(p0.x + 0.2f, p0.y, p0.z), Quaternion.Euler(new Vector3(0, 0, 90)));
        var TeamColour = gameObject.transform.Find("Circle").GetComponent<SpriteRenderer>().color;
        Arrow.GetComponent<SpriteRenderer>().color = TeamColour; 
        Arrow.GetComponent<NetworkObject>().Spawn();
    }

    public void Shoot(Transform target, int damage) {
        ArrowTime = 0;
        Target = target;
        TargetPosition = target.position;
        Damage = damage;
        DrawArrow();
    }

    private void DoDamage() {
        if (Vector3.Distance(TargetPosition, Arrow.transform.position) <= 0.1f) {
            Debug.Log("Arrow hits");
            Target.GetComponent<ISoldier>()?.TakeDamage(Damage);
        } else {
            Debug.Log("Arrow hits");
        }
        DestroyArrow();
    }

    private void DestroyArrow() {
        Destroy(Arrow, 3);
    }

    private void Update() {
        if (Arrow) {
            //Arrow.GetComponent<Rigidbody2D>().DOJump(Target, 0.2f, 1, 3).onComplete = DestroyArrow;
            Arrow.GetComponent<Rigidbody2D>().DOMove(TargetPosition, 2).onComplete = DoDamage;
            ArrowTime += Time.deltaTime;
        }
        if (ArrowTime >= TimeTillSelfdestruct) {
            Destroy(Arrow);
            return;
        }
    }
}
