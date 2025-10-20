using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace TowerDefense
{
	public class TowersManager : MonoBehaviour
	{
		private static TowersManager _instance;
		public static TowersManager Instance => _instance;

		[ShowInInspector] [ReadOnly]
		private readonly HashSet<TowerUnit> _registeredTowers = new HashSet<TowerUnit>();

		[ShowInInspector] [ReadOnly]
		private float _cachedTotalDPS = 0f;

		[ShowInInspector] [ReadOnly]
		private float _cachedCorrectedTotalDPS = 0f;

		[ShowInInspector] [ReadOnly]
		private float _lastDPSCalculationTime = 0f;

		private const float DPS_CALCULATION_INTERVAL = 1f;
		public float RELOAD_WORKER_TRAVEL_TIME = 6f;

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
			}
			else
			{
				Destroy(gameObject);
			}
		}

		private void Update()
		{
			if (Time.time >= _lastDPSCalculationTime + DPS_CALCULATION_INTERVAL)
			{
				CalculateTotalDPS();
				_lastDPSCalculationTime = Time.time;
			}
		}

	public static void RegisterTower(TowerUnit tower)
	{
		if (Instance != null && tower != null)
		{
			Instance._registeredTowers.Add(tower);
			
			var zombieManager = FindFirstObjectByType<ZombieManager>();
			if(zombieManager != null)
			{
				zombieManager.OnTowerBuilt();
			}
		}
	}

		public static void UnregisterTower(TowerUnit tower)
		{
			if (Instance != null && tower != null)
			{
				Instance._registeredTowers.Remove(tower);
			}
		}

		public int GetTowerCount()
		{
			return _registeredTowers.Count;
		}

		public int GetTowerGroupsWithTowersCount()
		{
			var towerGroups = FindObjectsByType<TowerGroup>(FindObjectsSortMode.None);
			var count = 0;
			foreach(var group in towerGroups)
			{
				if(group.HasAnyTowers())
				{
					count++;
				}
			}
			return count;
		}

		public float GetTotalDPS()
		{
			return _cachedTotalDPS;
		}

		public float GetCorrectedTotalDPS()
		{
			return _cachedCorrectedTotalDPS;
		}

		public List<TowerUnit> GetAllTowers()
		{
			return new List<TowerUnit>(_registeredTowers);
		}

		private void CalculateTotalDPS()
		{
			_cachedTotalDPS = 0f;
			_cachedCorrectedTotalDPS = 0f;

			var zombieManager = FindFirstObjectByType<ZombieManager>();
			var averageZombieHealth = GetAverageZombieHealth(zombieManager);

			foreach (var tower in _registeredTowers)
			{
				if (tower == null)
					continue;

				var towerDPS = CalculateTowerDPS(tower);
				var correctedTowerDPS = CalculateCorrectedTowerDPS(tower, averageZombieHealth);
				_cachedTotalDPS += towerDPS;
				_cachedCorrectedTotalDPS += correctedTowerDPS;
			}
		}

	private float CalculateTowerDPS(TowerUnit tower)
	{
		if (tower == null || tower.GetConfig() == null)
			return 0f;

		var config = tower.GetConfig();
		var totalClipDamage = tower.GetTotalClipDamage();
		var attackCooldown = config.AttackCooldown;
		var clipSize = config.ClipSize;
		var reloadTime = config.ReloadTime;

		var timeToEmptyClip = clipSize * attackCooldown;
		var totalCycleTime = timeToEmptyClip + reloadTime + RELOAD_WORKER_TRAVEL_TIME;

		var dps = totalClipDamage / totalCycleTime;
		return dps;
	}

	private float CalculateCorrectedTowerDPS(TowerUnit tower, float averageZombieHealth)
	{
		if (tower == null || tower.GetConfig() == null || averageZombieHealth <= 0f)
			return 0f;

		var config = tower.GetConfig();
		var damagePerShot = tower.GetDamagePerShot();
		var attackCooldown = config.AttackCooldown;
		var clipSize = config.ClipSize;
		var reloadTime = config.ReloadTime;

		var timeToEmptyClip = clipSize * attackCooldown;
		var totalCycleTime = timeToEmptyClip + reloadTime + RELOAD_WORKER_TRAVEL_TIME;

		float effectiveDamagePerClip;

		if (config.AttackType == AttackType.Single)
		{
			var effectiveDamagePerShot = Mathf.Min(damagePerShot, averageZombieHealth);
			effectiveDamagePerClip = effectiveDamagePerShot * clipSize;
		}
		else
		{
			var effectiveDamagePerBullet = Mathf.Min(damagePerShot, averageZombieHealth);
			effectiveDamagePerClip = effectiveDamagePerBullet * config.BulletsPerShot * clipSize;
		}

		var correctedDPS = effectiveDamagePerClip / totalCycleTime;
		return correctedDPS;
	}

		private float GetAverageZombieHealth(ZombieManager zombieManager)
		{
			if (zombieManager == null)
				return 100f;

			return zombieManager.GetCurrentInterpolatedHealth();
		}

		[Button("Recalculate DPS")]
		private void ForceRecalculateDPS()
		{
			CalculateTotalDPS();
		}

		[Button("Debug Tower Info")]
		private void DebugTowerInfo()
		{
			var zombieManager = FindFirstObjectByType<ZombieManager>();
			var averageZombieHealth = GetAverageZombieHealth(zombieManager);

			Debug.Log($"Total Towers: {GetTowerCount()}");
			Debug.Log($"Total DPS: {GetTotalDPS():F2}");
			Debug.Log($"Corrected Total DPS: {GetCorrectedTotalDPS():F2}");
			Debug.Log($"Average Zombie Health: {averageZombieHealth:F1}");

			foreach (var tower in _registeredTowers)
			{
				if (tower != null)
				{
					var towerDPS = CalculateTowerDPS(tower);
					var correctedTowerDPS = CalculateCorrectedTowerDPS(tower, averageZombieHealth);
					var config = tower.GetConfig();
					var attackType = config != null ? config.AttackType.ToString() : "Unknown";
					var bulletsPerShot = config?.BulletsPerShot ?? 1;

				Debug.Log($"Tower {tower.name} ({attackType}): DPS = {towerDPS:F2}, Corrected DPS = {correctedTowerDPS:F2}, " +
				         $"Total Clip Damage = {tower.GetTotalClipDamage()}, Damage/Shot = {tower.GetDamagePerShot()}, Bullets/Shot = {bulletsPerShot}, " +
				         $"Cooldown = {tower.GetAttackCooldown()}, Clip = {tower.GetClipSize()}, " +
				         $"Reload = {tower.GetReloadTime()}, Ammo = {tower.GetCurrentAmmo()}, " +
				         $"Reloading = {tower.IsReloading()}");
				}
			}
		}

		private void OnDestroy()
		{
			if (_instance == this)
			{
				_instance = null;
			}
		}
	}
}
