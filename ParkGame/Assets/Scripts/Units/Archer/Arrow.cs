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
        [SerializeField] private ColorSettings colorSettings;

        private int team;
        private int damage;
        private Transform target;
        private Vector3 positionOfTarget; // because Transform is directly on some gameobject !
        private float spawnTime;
        private bool dealtDamage;

        public void Initialize(Transform target, int damage, int team)
        {
            this.target = target;
            this.damage = damage;
            this.team = team;
            this.positionOfTarget = target.position;
            this.spawnTime = Time.time;
            
            initColorClientRPC();
        }
        
        [ClientRpc]
        void initColorClientRPC() {
            spriteRenderer.color = Color.clear;
            spriteRenderer.DOColor(Color.white, 0.1f).SetDelay(delay);
            
            feathersSpriteRenderer.color = Color.clear;
            Color teamColor = colorSettings.Colors[team].Color;
            feathersSpriteRenderer.DOColor(teamColor, 0.1f).SetDelay(delay);
        }

        private void Update() {
            if (!IsServer) return;
            if (this.spawnTime + delay > Time.time) return;

            // ARROW HITS
            if (Vector3.Distance(transform.position, positionOfTarget) < 0.1f && !dealtDamage) {
                Debug.Log("Arrow hits");

                Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(transform.position, 0.1f);
                foreach (var c in collider2Ds) {
                    if (c.gameObject.GetComponent<ISoldier>() != null) {
                        ISoldier hitTarget = c.gameObject.GetComponent<ISoldier>();

                        // find first hit enemy soldier, deal him damage, destruct arrow
                        if (hitTarget.Team != team) {
                            Debug.Log(team + " hits " + hitTarget.Team);
                            hitTarget.TakeDamage(damage);
                            dealtDamage = true;
                            Destroy(gameObject);
                            return;
                        }
                    }
                }


                if (Vector3.Distance(transform.position, positionOfTarget) < 0.01f) {
                    dealtDamage = true; 
                    Destroy(gameObject);

                    return;
                }
                /*
                    // Cast a ray straight down.
                    RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);

                    // If it hits something...
                    if (hit.collider != null) {
                        // Calculate the distance from the surface and the "error" relative
                        // to the floating height.
                        float distance = Mathf.Abs(hit.point.y - transform.position.y);

                        //Destroy(gameObject);
                        Debug.Log("ARROW HITS " + hit.collider.gameObject.name + " " + distance);

            }
            */
                // but what if it hits someone else instead??? -> control position of enemies
                // have all soldiers in some quad tree ?
            }
       

            //Vector3 targetPosition = target.position;
            //Vector3 toTarget = (targetPosition - transform.position).normalized;
            Vector3 toTarget = (positionOfTarget - transform.position).normalized;
            transform.position += toTarget * (moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg);
            
          
        }
    }
}