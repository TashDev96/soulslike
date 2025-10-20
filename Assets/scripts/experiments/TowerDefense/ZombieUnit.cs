using System;
using System.Collections.Generic;
using RVO;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using Unity.Mathematics;
using UnityEngine;

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
		public Action<ZombieUnit, float> OnTakeDamage;

		private readonly List<PendingDamage> _pendingDamages = new();
		public float MaxHealth { get; }
		public float CurrentHealth { get; private set; }
		public bool IsDead => CurrentHealth <= 0f;

	public bool HasPendingDamage => _pendingDamages.Count > 0;

	public ZombieUnit(Vector3 startPosition, BakedMeshSequence meshSequence, Material material,
		float health = 100f, int layer = 0, float scale = 1f)
		: base(startPosition, meshSequence, material, layer, scale)
	{
		MaxHealth = health;
		CurrentHealth = health;
	}

		public void TakeDamage(float damage, float delay = 0f, TowerUnit damageDealer = null)
		{
			if(IsDead)
			{
				return;
			}

			if(delay > 0f)
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
			for(var i = _pendingDamages.Count - 1; i >= 0; i--)
			{
				var pendingDamage = _pendingDamages[i];
				if(Time.time >= pendingDamage.applyTime)
				{
					_pendingDamages.RemoveAt(i);
					ApplyDamage(pendingDamage.damage);
				}
			}
		}

		public bool IsNearTheWall => Position.x < -0.5f;
		protected override float ZSpeedMult => IsNearTheWall ? 0.05f : 1f;

		public override void UpdateAgent(Simulator simulator, float2 goal, float deltaTime)
		{
			base.UpdateAgent(simulator, goal, deltaTime);
		}

		public bool HasPendingDamageFrom(TowerUnit tower)
		{
			for(var i = 0; i < _pendingDamages.Count; i++)
			{
				if(_pendingDamages[i].damageDealer == tower)
				{
					return true;
				}
			}
			return false;
		}

		public void Heal(float amount)
		{
			if(IsDead)
			{
				return;
			}
			CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
		}

		public float GetHealthPercentage()
		{
			return MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
		}

		public bool WouldDieFromDelayedDamage()
		{
			var totalPendingDamage = 0f;
			for(var i = 0; i < _pendingDamages.Count; i++)
			{
				totalPendingDamage += _pendingDamages[i].damage;
			}
			return CurrentHealth <= totalPendingDamage;
		}

	private void ApplyDamage(float damage)
	{
		if(IsDead)
		{
			return;
		}

		CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
		OnTakeDamage?.Invoke(this, damage);
	}
	}
}
