using experiments;
using UnityEngine;
using RVO;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;

namespace TowerDefense
{
    public class ZombieUnit : RVOAgent
    {
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;
        public int MoneyValue { get; private set; }
        public int KillBonusValue { get; private set; }
        
        public System.Action<ZombieUnit> OnDeath;
        public System.Action<ZombieUnit, float> OnTakeDamage;

        public ZombieUnit(Vector3 startPosition, BakedMeshSequence meshSequence, Material material, 
                         float health = 100f, int moneyValue = 10, int killBonus = 50, int layer = 0) 
            : base(startPosition, meshSequence, material, layer)
        {
            MaxHealth = health;
            CurrentHealth = health;
            MoneyValue = moneyValue;
            KillBonusValue = killBonus;
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            OnTakeDamage?.Invoke(this, damage);

            if (IsDead)
            {
                OnDeath?.Invoke(this);
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        public float GetHealthPercentage()
        {
            return MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
        }

       
    }
}
