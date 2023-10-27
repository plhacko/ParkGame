using DG.Tweening;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Units.Archer
{
    public class Arrow : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float delay;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer feathersSpriteRenderer;
        
        private int damage;
        private Transform target;
        private float spawnTime;
        
        public void Initialize(Transform target, int damage)
        {
            this.target = target;
            this.damage = damage;
            this.spawnTime = Time.time;
            
            initColorClientRPC();
        }
        
        [ClientRpc]
        void initColorClientRPC() {
            spriteRenderer.color = Color.clear;
            spriteRenderer.DOColor(Color.white, 0.1f).SetDelay(delay);
            
            feathersSpriteRenderer.color = Color.clear;
            feathersSpriteRenderer.DOColor(Color.white, 0.1f).SetDelay(delay);
        }
        
        private void Update()
        {
            if(!IsServer) return;
            if(this.spawnTime + delay > Time.time) return;
            if(target == null)
            {
                Destroy(gameObject);
                return;
            }
            
            Vector3 targetPosition = target.position;
            Vector3 toTarget = (targetPosition - transform.position).normalized;
            transform.position += toTarget * (moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg);
            
            if(Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                Debug.Log("Arrow hits");
                target.GetComponent<ISoldier>()?.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}