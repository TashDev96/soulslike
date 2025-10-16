using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerDefense
{
	public enum AttackType
	{
		Single,
		Shotgun
	}

	public class TowerUnit : MonoBehaviour
	{
		[Header("Tower Configuration")]
		[SerializeField]
		private TowerConfig config;

		[Header("Visual")]
		[SerializeField]
		private Transform rotationPivot;
		[SerializeField]
		private LineRenderer attackLineRenderer;
		[SerializeField]
		private LineRenderer[] shotgunLineRenderers;
		[SerializeField]
		private float attackLineDisplayTime = 0.5f;

		[Header("Runtime Settings")]
		[SerializeField]
		private LayerMask zombieLayerMask = -1;

		public Action<TowerUnit, ZombieUnit, float> OnAttackHit;

		private float _lastAttackTime;
		private float _lastReloadTime;
		private GameManager _gameManager;
		private int _currentUpgradeLevel;
		[ShowInInspector]
		private int _currentAmmo;
		[ShowInInspector]
		private bool _isReloading;
		private ZombieUnit _currentTarget;

		private void Start()
		{
			_gameManager = FindFirstObjectByType<GameManager>();

			TargetingManager.RegisterTower(this);

			if(config != null)
			{
				_currentAmmo = config.ClipSize;
			}

		if(attackLineRenderer != null)
		{
			attackLineRenderer.enabled = false;
			attackLineRenderer.startWidth = 0.1f;
			attackLineRenderer.endWidth = 0.05f;
			attackLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
			attackLineRenderer.startColor = Color.red;
			attackLineRenderer.endColor = Color.red;
		}

		CreateShotgunLineRenderers();
	}

	private void CreateShotgunLineRenderers()
	{
		if(config == null || attackLineRenderer == null)
		{
			return;
		}

		if(config.AttackType == AttackType.Shotgun)
		{
			var bulletsPerShot = config.BulletsPerShot;
			shotgunLineRenderers = new LineRenderer[bulletsPerShot];

			for(var i = 0; i < bulletsPerShot; i++)
			{
				var clonedGameObject = Instantiate(attackLineRenderer.gameObject, attackLineRenderer.transform.parent);
				clonedGameObject.name = $"ShotgunLineRenderer_{i}";
				
				var clonedLineRenderer = clonedGameObject.GetComponent<LineRenderer>();
				clonedLineRenderer.enabled = false;
				
				shotgunLineRenderers[i] = clonedLineRenderer;
			}
		}
	}

	public float GetAttackRange()
		{
			return config?.AttackRange ?? 5f;
		}

		public float GetAttackCooldown()
		{
			return config?.AttackCooldown ?? 1f;
		}

		public float GetReloadTime()
		{
			return config?.ReloadTime ?? 2f;
		}

		public int GetClipSize()
		{
			return config?.ClipSize ?? 10;
		}

		public int GetCurrentAmmo()
		{
			return _currentAmmo;
		}

		public bool IsReloading()
		{
			return _isReloading;
		}

		public void ForceReload()
		{
			if(!_isReloading)
			{
				StartReload();
			}
		}

		public void ReloadWithAmmo(int ammoAmount)
		{
			_currentAmmo = Mathf.Min(_currentAmmo + ammoAmount, GetClipSize());
			_isReloading = false;
		}

		public float GetCurrentDamage()
		{
			if(config == null)
			{
				return 25f;
			}

			var totalDamage = config.BaseDamage;

			if(config.UpgradeLevels != null && _currentUpgradeLevel > 0 && _currentUpgradeLevel <= config.UpgradeLevels.Count)
			{
				for(var i = 0; i < _currentUpgradeLevel; i++)
				{
					if(i < config.UpgradeLevels.Count)
					{
						totalDamage += config.UpgradeLevels[i].DamageBonus;
					}
				}
			}

			return totalDamage;
		}

		public bool CanUpgrade()
		{
			return config != null && config.UpgradeLevels != null && _currentUpgradeLevel < config.UpgradeLevels.Count;
		}

		public int GetUpgradePrice()
		{
			if(!CanUpgrade())
			{
				return 0;
			}
			return config.UpgradeLevels[_currentUpgradeLevel].LevelUpPrice;
		}

		public void UpgradeLevel()
		{
			if(CanUpgrade())
			{
				_currentUpgradeLevel++;
			}
		}

		public int GetCurrentUpgradeLevel()
		{
			return _currentUpgradeLevel;
		}

		public TowerConfig GetConfig()
		{
			return config;
		}

		private void Update()
		{
			if(CanAttack())
			{
				var target = FindNearestZombie();

				if(target != null)
				{
					_currentTarget = target;
					AttackTarget(target);
				}
			}
		}

		private bool CanAttack()
		{
			if(config == null)
			{
				return false;
			}
			if(_isReloading)
			{
				return false;
			}
			if(_currentAmmo <= 0)
			{
				return false;
			}
			return Time.time >= _lastAttackTime + config.AttackCooldown;
		}

		private bool NeedsReload()
		{
			return _currentAmmo <= 0;
		}

		private void StartReload()
		{
			_isReloading = true;
			_lastReloadTime = Time.time;
		}

		private ZombieUnit FindNearestZombie()
		{
			return TargetingManager.RequestTarget(this, config.AttackRange);
		}

		private void AttackTarget(ZombieUnit target)
		{
			_lastAttackTime = Time.time;

			if(rotationPivot != null)
			{
				var directionToTarget = (target.Position - transform.position).normalized;
				rotationPivot.rotation = Quaternion.LookRotation(directionToTarget);
			}

			var currentDamage = GetCurrentDamage();

			if(config == null)
			{
				return;
			}

			_currentAmmo--;

			switch(config.AttackType)
			{
				case AttackType.Single:
					AttackSingle(target, currentDamage);
					break;
				case AttackType.Shotgun:
					AttackShotgun(target, currentDamage);
					break;
			}
		}

		private void AttackSingle(ZombieUnit target, float damage)
		{
			target.TakeDamage(damage, config.DamageDelay, this);
			OnAttackHit?.Invoke(this, target, damage);

			if(_gameManager != null)
			{
				_gameManager.OnZombieHit(target, damage);
			}

			ShowAttackLine(target.Position);
		}

		private void AttackShotgun(ZombieUnit primaryTarget, float damage)
		{
			var zombies = TargetingManager.GetAliveZombies();
			var towerPos = transform.position;
			var extendedRange = config.AttackRange + 2f;
			var extendedRangeSq = extendedRange * extendedRange;
			var directionToPrimary = (primaryTarget.Position - towerPos).normalized;
			var shotgunAngle = config.ShotgunAngle;

			TargetingManager.TempShotgunTargets.Clear();

			for(var i = 0; i < zombies.Count; i++)
			{
				var zombie = zombies[i];

				if(zombie.WouldDieFromDelayedDamage())
				{
					continue;
				}

				var zombiePos = zombie.Position;

				if(Mathf.Abs(zombiePos.x - towerPos.x) > extendedRange)
				{
					continue;
				}

				var towerToZombie = zombiePos - towerPos;
				var distanceSq = towerToZombie.sqrMagnitude;

				if(distanceSq > extendedRangeSq)
				{
					continue;
				}

				var directionToZombie = towerToZombie.normalized;
				var angle = Vector3.Angle(directionToPrimary, directionToZombie);

				if(angle <= shotgunAngle * 0.5f)
				{
					TargetingManager.TempShotgunTargets.Add((zombie, distanceSq));
				}
			}

			TargetingManager.TempShotgunTargets.Sort((a, b) => a.distanceSq.CompareTo(b.distanceSq));

			var bulletsToFire = Mathf.Min(config.BulletsPerShot, TargetingManager.TempShotgunTargets.Count);
			var selectedTargets = new List<ZombieUnit>();

			for(var i = 0; i < bulletsToFire; i++)
			{
				selectedTargets.Add(TargetingManager.TempShotgunTargets[i].zombie);
			}

			var bulletDistribution = new int[selectedTargets.Count];
			for(var i = 0; i < config.BulletsPerShot; i++)
			{
				if(selectedTargets.Count > 0)
				{
					var randomIndex = Random.Range(0, selectedTargets.Count);
					bulletDistribution[randomIndex]++;
				}
			}

			var hitTargetPositions = new List<Vector3>();

			for(var i = 0; i < selectedTargets.Count; i++)
			{
				var zombie = selectedTargets[i];
				var bulletsHit = bulletDistribution[i];

				if(bulletsHit > 0)
				{
					var totalDamage = damage * bulletsHit;
					zombie.TakeDamage(totalDamage, config.DamageDelay, this);
					OnAttackHit?.Invoke(this, zombie, totalDamage);

					if(_gameManager != null)
					{
						_gameManager.OnZombieHit(zombie, totalDamage);
					}

					hitTargetPositions.Add(zombie.Position);
				}
			}

			ShowShotgunAttackLines(hitTargetPositions);
		}

		private void ShowAttackLine(Vector3 targetPosition)
		{
			if(attackLineRenderer != null)
			{
				StartCoroutine(DisplayAttackLine(targetPosition));
			}
		}

		private void ShowShotgunAttackLines(List<Vector3> targetPositions)
		{
			if(shotgunLineRenderers != null && targetPositions.Count > 0)
			{
				StartCoroutine(DisplayShotgunAttackLines(targetPositions));
			}
			else if(attackLineRenderer != null && targetPositions.Count > 0)
			{
				StartCoroutine(DisplayAttackLine(targetPositions[0]));
			}
		}

		private IEnumerator DisplayAttackLine(Vector3 targetPosition)
		{
			attackLineRenderer.enabled = true;
			attackLineRenderer.SetPosition(0, transform.position + Vector3.up);
			attackLineRenderer.SetPosition(1, targetPosition + Vector3.up);

			yield return new WaitForSeconds(config.VfxDuration);

			attackLineRenderer.enabled = false;
		}

		private IEnumerator DisplayShotgunAttackLines(List<Vector3> targetPositions)
		{
			var linesToShow = Mathf.Min(targetPositions.Count, shotgunLineRenderers.Length);

			for(var i = 0; i < linesToShow; i++)
			{
				if(shotgunLineRenderers[i] != null)
				{
					shotgunLineRenderers[i].enabled = true;
					shotgunLineRenderers[i].SetPosition(0, transform.position + Vector3.up);
					shotgunLineRenderers[i].SetPosition(1, targetPositions[i] + Vector3.up);
				}
			}

			yield return new WaitForSeconds(attackLineDisplayTime);

			for(var i = 0; i < linesToShow; i++)
			{
				if(shotgunLineRenderers[i] != null)
				{
					shotgunLineRenderers[i].enabled = false;
				}
			}
		}

		private void OnDestroy()
		{
			TargetingManager.UnregisterTower(this);
		}

	private void OnDrawGizmosSelected()
	{
		if(config != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, config.AttackRange);

			if(config.AttackType == AttackType.Shotgun)
			{
				Gizmos.color = Color.yellow;
				var halfAngle = config.ShotgunAngle * 0.5f;
				var range = config.AttackRange;
				var center = transform.position;
				
				var rightDirection = Quaternion.AngleAxis(halfAngle, Vector3.up) * Vector3.right;
				var leftDirection = Quaternion.AngleAxis(-halfAngle, Vector3.up) * Vector3.right;
				
				Gizmos.DrawLine(center, center + rightDirection * range);
				Gizmos.DrawLine(center, center + leftDirection * range);
				Gizmos.DrawLine(center, center + Vector3.right * range);
			}
		}
	}
	}
}
