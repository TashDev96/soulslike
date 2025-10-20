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
	private List<LineRenderer> _pooledLineRenderers = new List<LineRenderer>();
	private int _activeLineCoroutines = 0;

		public void Initialize(TowerConfig towerConfig)
		{
			config = towerConfig;
			_gameManager = FindFirstObjectByType<GameManager>();

			TargetingManager.RegisterTower(this);
			TowersManager.RegisterTower(this);

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

		public void SetUpgradeLevel(int level)
		{
			_currentUpgradeLevel = level;
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

	public float GetTotalClipDamage()
	{
		if(config == null)
		{
			return 25f;
		}

		var totalDamage = 1f;

		if(config.UpgradeLevels != null && _currentUpgradeLevel < config.UpgradeLevels.Count)
		{
			totalDamage = config.UpgradeLevels[_currentUpgradeLevel].BaseDamage;
		}

		var damageMultiplier = config.DamageMultiplier;
		if(config.UpgradeLevels != null && _currentUpgradeLevel < config.UpgradeLevels.Count)
		{
			var multiplier = config.UpgradeLevels[_currentUpgradeLevel].DamageMultiplier;
			if(multiplier > 0)
			{
				damageMultiplier += multiplier;
			}
		}

		return totalDamage * damageMultiplier;
	}

	public float GetDamagePerShot()
	{
		var totalClipDamage = GetTotalClipDamage();
		var clipSize = GetClipSize();
		
		if(config.AttackType == AttackType.Shotgun)
		{
			return totalClipDamage / (clipSize * config.BulletsPerShot);
		}
		
		return totalClipDamage / clipSize;
	}

		public int GetCurrentUpgradeLevel()
		{
			return _currentUpgradeLevel;
		}

		public TowerConfig GetConfig()
		{
			return config;
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

		private void Update()
		{
			if(_currentAmmo <= 0 && _currentTarget != null)
			{
				TargetingManager.ClearTowerTarget(this);
				_currentTarget = null;
			}

			if(CanAttack() && _currentAmmo > 0)
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

			if(_currentTarget != null)
			{
				TargetingManager.ClearTowerTarget(this);
				_currentTarget = null;
			}
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

			var damagePerShot = GetDamagePerShot();

			if(config == null)
			{
				return;
			}

			_currentAmmo--;

			switch(config.AttackType)
			{
				case AttackType.Single:
					AttackSingle(target, damagePerShot);
					break;
				case AttackType.Shotgun:
					AttackShotgun(target, damagePerShot);
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

		private void AttackShotgun(ZombieUnit primaryTarget, float damagePerBullet)
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

			var availableTargets = new List<ZombieUnit>();
			for(var i = 0; i < TargetingManager.TempShotgunTargets.Count; i++)
			{
				availableTargets.Add(TargetingManager.TempShotgunTargets[i].zombie);
			}

			var hitTargetPositions = new List<Vector3>();

			for(var bulletIndex = 0; bulletIndex < config.BulletsPerShot; bulletIndex++)
			{
				var validTargets = new List<ZombieUnit>();
				for(var i = 0; i < availableTargets.Count; i++)
				{
					var zombie = availableTargets[i];
					if(!zombie.WouldDieFromDelayedDamage())
					{
						validTargets.Add(zombie);
					}
				}

				if(validTargets.Count == 0)
				{
					break;
				}

				var randomIndex = Random.Range(0, validTargets.Count);
				var targetZombie = validTargets[randomIndex];

				targetZombie.TakeDamage(damagePerBullet, config.DamageDelay, this);
				OnAttackHit?.Invoke(this, targetZombie, damagePerBullet);

				if(_gameManager != null)
				{
					_gameManager.OnZombieHit(targetZombie, damagePerBullet);
				}

				if(!hitTargetPositions.Contains(targetZombie.Position))
				{
					hitTargetPositions.Add(targetZombie.Position);
				}
			}

			ShowShotgunAttackLines(hitTargetPositions);
		}

	private void ShowAttackLine(Vector3 targetPosition)
	{
		if(attackLineRenderer != null)
		{
			LineRenderer lineToUse;
			
			if(_activeLineCoroutines > 0)
			{
				lineToUse = GetOrCreatePooledLineRenderer();
			}
			else
			{
				lineToUse = attackLineRenderer;
			}
			
			StartCoroutine(DisplayAttackLine(targetPosition, lineToUse));
		}
	}

	private LineRenderer GetOrCreatePooledLineRenderer()
	{
		foreach(var pooledLine in _pooledLineRenderers)
		{
			if(pooledLine != null && !pooledLine.enabled)
			{
				return pooledLine;
			}
		}
		
		var newLine = Instantiate(attackLineRenderer.gameObject, attackLineRenderer.transform.parent).GetComponent<LineRenderer>();
		newLine.name = $"PooledAttackLine_{_pooledLineRenderers.Count}";
		newLine.enabled = false;
		_pooledLineRenderers.Add(newLine);
		
		return newLine;
	}

	private void ShowShotgunAttackLines(List<Vector3> targetPositions)
	{
		if(shotgunLineRenderers != null && targetPositions.Count > 0)
		{
			StartCoroutine(DisplayShotgunAttackLines(targetPositions));
		}
		else if(attackLineRenderer != null && targetPositions.Count > 0)
		{
			ShowAttackLine(targetPositions[0]);
		}
	}

	private IEnumerator DisplayAttackLine(Vector3 targetPosition, LineRenderer lineRenderer)
	{
		_activeLineCoroutines++;
		
		lineRenderer.enabled = true;
		lineRenderer.SetPosition(0, transform.position + Vector3.up);
		
		var spread = Random.insideUnitCircle * 0.3f;
		var endPosition = targetPosition + Vector3.up + new Vector3(spread.x, 0f, spread.y);
		lineRenderer.SetPosition(1, endPosition);

		yield return new WaitForSeconds(config.VfxDuration);

		lineRenderer.enabled = false;
		
		_activeLineCoroutines--;
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
				
				var spread = Random.insideUnitCircle * 0.3f;
				var endPosition = targetPositions[i] + Vector3.up + new Vector3(spread.x, 0f, spread.y);
				shotgunLineRenderers[i].SetPosition(1, endPosition);
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
			TowersManager.UnregisterTower(this);
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
