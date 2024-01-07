using DG.Tweening;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Units.Archer {
    public class Arrow : NetworkBehaviour {
        [SerializeField] private float moveSpeed;
        [SerializeField] private float delay;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer feathersSpriteRenderer;
        [SerializeField] private ColorSettings colorSettings;

        private int team;
        private int damage;
        private Vector3 positionOfTarget;
        private float spawnTime;
        private bool dealtDamage;

        public void Initialize(Vector3 targetPosition, int damage, int team) {
            this.damage = damage;
            this.team = team;
            this.positionOfTarget = targetPosition;
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

        void Hit() {
            Debug.Log("Arrow hits");

            Collider2D[] collider2Ds = Physics2D.OverlapCircleAll(transform.position, 0.1f);
            foreach (var c in collider2Ds) {
                if (c.gameObject.GetComponent<ISoldier>() != null) {
                    ISoldier hitTarget = c.gameObject.GetComponent<ISoldier>();

                    // find first hit enemy soldier, deal him damage, destruct arrow
                    if (hitTarget.Team != team) {
                        Debug.Log(team + " hits " + hitTarget.Team);
                        hitTarget.TakeDamage(damage);
                        return;
                    }
                }
            }
        }

        private void Update() {
            if (!IsServer) return;
            if (this.spawnTime + delay > Time.time) return;

            // ARROW HITS
            if (Vector3.Distance(transform.position, positionOfTarget) < 0.1f) {
                Hit();
                Destroy(gameObject);
                return;
            
            }

            Vector3 toTarget = (positionOfTarget - transform.position).normalized;
            transform.position += toTarget * (moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg);
        }
    }
}