using System;
using System.Collections;
using System.Collections.Generic;
using experiments;
using UnityEngine;
using RVO;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;

namespace TowerDefense
{
    public struct PendingDamage
    {
        public float damage;
        public float applyTime;
        public TowerUnit damageDealer;
        
        public PendingDamage(float damage, float applyTime, TowerUnit damageDealer)
        {
            this.damage = damage;
            this.applyTime = applyTime;
            this.damageDealer = damageDealer;
        }
    }
    public class ZombieUnit : RVOAgent
    {
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0f;
        public int MoneyValue { get; private set; }
        public int KillBonusValue { get; private set; }
        
        public System.Action<ZombieUnit> OnDeath;
        public System.Action<ZombieUnit, float> OnTakeDamage;
        
        private List<PendingDamage> _pendingDamages = new List<PendingDamage>();

        public ZombieUnit(Vector3 startPosition, BakedMeshSequence meshSequence, Material material, 
                         float health = 100f, int moneyValue = 10, int killBonus = 50, int layer = 0) 
            : base(startPosition, meshSequence, material, layer)
        {
            MaxHealth = health;
            CurrentHealth = health;
            MoneyValue = moneyValue;
            KillBonusValue = killBonus;
        }

        public void TakeDamage(float damage, float delay = 0f, TowerUnit damageDealer = null)
        {
            if (IsDead) return;

            if (delay > 0f)
            {
                var pendingDamage = new PendingDamage(damage, Time.time + delay, damageDealer);
                _pendingDamages.Add(pendingDamage);
            }
            else
            {
                ApplyDamage(damage);
            }
        }
        
        public void UpdateDelayedDamage()
        {
            for (int i = _pendingDamages.Count - 1; i >= 0; i--)
            {
                var pendingDamage = _pendingDamages[i];
                if (Time.time >= pendingDamage.applyTime)
                {
                    _pendingDamages.RemoveAt(i);
                    ApplyDamage(pendingDamage.damage);
                }
            }
        }
        
        private void ApplyDamage(float damage)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            Debug.LogError(CurrentHealth);
            OnTakeDamage?.Invoke(this, damage);

            if (IsDead)
            {
                OnDeath?.Invoke(this);
            }
        }
        
        public bool HasPendingDamage => _pendingDamages.Count > 0;
        
        public bool HasPendingDamageFrom(TowerUnit tower)
        {
            for (int i = 0; i < _pendingDamages.Count; i++)
            {
                if (_pendingDamages[i].damageDealer == tower)
                {
                    return true;
                }
            }
            return false;
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

        public bool WouldDieFromDelayedDamage()
        {
            float totalPendingDamage = 0f;
            for (int i = 0; i < _pendingDamages.Count; i++)
            {
                totalPendingDamage += _pendingDamages[i].damage;
            }
            return CurrentHealth <= totalPendingDamage;
        }

       
    }
}
