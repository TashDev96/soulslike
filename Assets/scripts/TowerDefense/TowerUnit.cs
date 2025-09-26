using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TowerDefense
{
    public class TowerUnit : MonoBehaviour
    {
        [Header("Tower Stats")]
        [SerializeField] private float attackRange = 5f;
        [SerializeField] private float attackDamage = 25f;
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private LayerMask zombieLayerMask = -1;

        [Header("Visual")]
        [SerializeField] private Transform rotationPivot;
        [SerializeField] private LineRenderer attackLineRenderer;
        [SerializeField] private float attackLineDisplayTime = 0.1f;

        private float lastAttackTime;
        private ZombieManager zombieManager;
        private GameManager gameManager;

        public System.Action<TowerUnit, ZombieUnit, float> OnAttackHit;

        private void Start()
        {
            zombieManager = FindFirstObjectByType<ZombieManager>();
            gameManager = FindFirstObjectByType<GameManager>();
            
            if (attackLineRenderer != null)
            {
                attackLineRenderer.enabled = false;
                attackLineRenderer.startWidth = 0.1f;
                attackLineRenderer.endWidth = 0.05f;
                attackLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                attackLineRenderer.startColor = Color.red;
                attackLineRenderer.endColor = Color.red;
            }
        }

        private void Update()
        {
            if (CanAttack())
            {
                var target = FindNearestZombie();
                if (target != null)
                {
                    AttackTarget(target);
                }
            }
        }

        private bool CanAttack()
        {
            return Time.time >= lastAttackTime + attackCooldown;
        }

        private ZombieUnit FindNearestZombie()
        {
            if (zombieManager == null) return null;

            var zombies = zombieManager.GetAliveZombies();
            ZombieUnit nearestZombie = null;
            float nearestDistance = float.MaxValue;

            foreach (var zombie in zombies)
            {
                float distance = Vector3.Distance(transform.position, zombie.Position);
                if (distance <= attackRange && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestZombie = zombie;
                }
            }

            return nearestZombie;
        }

        private void AttackTarget(ZombieUnit target)
        {
            lastAttackTime = Time.time;

            if (rotationPivot != null)
            {
                Vector3 directionToTarget = (target.Position - transform.position).normalized;
                rotationPivot.rotation = Quaternion.LookRotation(directionToTarget);
            }

            target.TakeDamage(attackDamage);
            OnAttackHit?.Invoke(this, target, attackDamage);

            if (gameManager != null)
            {
                gameManager.OnZombieHit(target, attackDamage);
            }

            ShowAttackLine(target.Position);
        }

        private void ShowAttackLine(Vector3 targetPosition)
        {
            if (attackLineRenderer != null)
            {
                StartCoroutine(DisplayAttackLine(targetPosition));
            }
        }

        private System.Collections.IEnumerator DisplayAttackLine(Vector3 targetPosition)
        {
            attackLineRenderer.enabled = true;
            attackLineRenderer.SetPosition(0, transform.position + Vector3.up);
            attackLineRenderer.SetPosition(1, targetPosition + Vector3.up);
            
            yield return new WaitForSeconds(attackLineDisplayTime);
            
            attackLineRenderer.enabled = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        public void SetAttackRange(float range)
        {
            attackRange = range;
        }

        public void SetAttackDamage(float damage)
        {
            attackDamage = damage;
        }

        public void SetAttackCooldown(float cooldown)
        {
            attackCooldown = cooldown;
        }

        public float GetAttackRange() => attackRange;
        public float GetAttackDamage() => attackDamage;
        public float GetAttackCooldown() => attackCooldown;
    }
}
