using System.Collections.Generic;
using TowerDefense;
using UnityEngine;

public static class TargetingManager
{
	private static readonly Dictionary<TowerUnit, ZombieUnit> towerTargets = new();
	private static readonly Dictionary<ZombieUnit, TowerUnit> zombieTargeters = new();
	private static readonly HashSet<TowerUnit> registeredTowers = new();
	private static ZombieManager zombieManager;

	private static readonly Dictionary<int, List<ZombieUnit>> spatialGrid = new();
	private static readonly List<ZombieUnit> cachedAliveZombies = new();
	private static readonly List<ZombieUnit> tempTargetsInRange = new();
	private static readonly HashSet<int> tempGridCells = new();
	private static readonly List<(ZombieUnit zombie, float distanceSq)> tempShotgunTargets = new();
	private static readonly Dictionary<ZombieUnit, int> zombieValidationCache = new();

	private static float gridCellSize = 10f;
	private static int lastAliveZombieUpdateFrame = -1;
	private static int validationCacheFrame = -1;

	internal static List<(ZombieUnit zombie, float distanceSq)> TempShotgunTargets => tempShotgunTargets;

	public static void Initialize(ZombieManager zombieManagerInstance)
	{
		zombieManager = zombieManagerInstance;
		gridCellSize = 10f;
	}

	public static void RegisterTower(TowerUnit tower)
	{
		if(tower != null && !registeredTowers.Contains(tower))
		{
			registeredTowers.Add(tower);
		}
	}

	public static void UnregisterTower(TowerUnit tower)
	{
		if(tower == null)
		{
			return;
		}

		registeredTowers.Remove(tower);
		ClearTowerTarget(tower);
	}

	public static ZombieUnit RequestTarget(TowerUnit tower, float maxRange)
	{
		if(tower == null || zombieManager == null)
		{
			return null;
		}

		var currentTarget = GetCurrentTarget(tower);
		if(currentTarget != null && IsZombieValidForTower(currentTarget, tower))
		{
			var towerPosition = tower.transform.position;
			var distanceToCurrentTarget = (towerPosition - currentTarget.Position).sqrMagnitude;
			if(distanceToCurrentTarget <= maxRange * maxRange)
			{
				return currentTarget;
			}
		}

		UpdateCachedAliveZombies();

		if(cachedAliveZombies.Count == 0)
		{
			ClearTowerTarget(tower);
			return null;
		}

		var towerPos = tower.transform.position;
		var maxRangeSq = maxRange * maxRange;

		GetNeighborGridKeys(towerPos, maxRange, tempGridCells);
		tempTargetsInRange.Clear();

		foreach(var key in tempGridCells)
		{
			if(spatialGrid.TryGetValue(key, out var zombiesInCell))
			{
				for(var i = 0; i < zombiesInCell.Count; i++)
				{
					var zombie = zombiesInCell[i];
					if(IsZombieValidForTower(zombie, tower))
					{
						var distanceSq = (towerPos - zombie.Position).sqrMagnitude;
						if(distanceSq <= maxRangeSq)
						{
							tempTargetsInRange.Add(zombie);
						}
					}
				}
			}
		}

		if(tempTargetsInRange.Count == 0)
		{
			ClearTowerTarget(tower);
			return null;
		}

		ZombieUnit bestTarget = null;
		var closestDistanceSq = float.MaxValue;
		var leastTargeters = int.MaxValue;

		for(var i = 0; i < tempTargetsInRange.Count; i++)
		{
			var zombie = tempTargetsInRange[i];
			var targeterCount = GetTargeterCount(zombie);

			if(targeterCount == 0)
			{
				var distanceSq = (towerPos - zombie.Position).sqrMagnitude;
				if(distanceSq < closestDistanceSq)
				{
					bestTarget = zombie;
					closestDistanceSq = distanceSq;
				}
			}
			else if(targeterCount < leastTargeters)
			{
				bestTarget = zombie;
				leastTargeters = targeterCount;
				closestDistanceSq = (towerPos - zombie.Position).sqrMagnitude;
			}
			else if(targeterCount == leastTargeters)
			{
				var distanceSq = (towerPos - zombie.Position).sqrMagnitude;
				if(distanceSq < closestDistanceSq)
				{
					bestTarget = zombie;
					closestDistanceSq = distanceSq;
				}
			}
		}

		if(bestTarget != null)
		{
			SetTowerTarget(tower, bestTarget);
		}

		return bestTarget;
	}

	public static ZombieUnit GetCurrentTarget(TowerUnit tower)
	{
		return tower != null && towerTargets.TryGetValue(tower, out var target) ? target : null;
	}

	public static bool IsZombieTargeted(ZombieUnit zombie)
	{
		return zombie != null && zombieTargeters.ContainsKey(zombie);
	}

	public static int GetTargeterCount(ZombieUnit zombie)
	{
		return zombie != null && zombieTargeters.ContainsKey(zombie) ? 1 : 0;
	}

	public static void ClearTowerTarget(TowerUnit tower)
	{
		if(tower == null)
		{
			return;
		}

		if(towerTargets.TryGetValue(tower, out var currentTarget))
		{
			towerTargets.Remove(tower);

			if(currentTarget != null && zombieTargeters.TryGetValue(currentTarget, out var targeter) && targeter == tower)
			{
				zombieTargeters.Remove(currentTarget);
			}
		}
	}

	public static void OnZombieDied(ZombieUnit zombie)
	{
		if(zombie == null)
		{
			return;
		}

		cachedAliveZombies.Remove(zombie);
		zombieValidationCache.Remove(zombie);

		if(zombieTargeters.TryGetValue(zombie, out var targeter))
		{
			towerTargets.Remove(targeter);
			zombieTargeters.Remove(zombie);
		}
	}

	public static void ClearAllTargets()
	{
		towerTargets.Clear();
		zombieTargeters.Clear();
		cachedAliveZombies.Clear();
		spatialGrid.Clear();
		zombieValidationCache.Clear();
		lastAliveZombieUpdateFrame = -1;
		validationCacheFrame = -1;
	}

	public static Dictionary<TowerUnit, ZombieUnit> GetAllTargetAssignments()
	{
		return new Dictionary<TowerUnit, ZombieUnit>(towerTargets);
	}

	public static int GetRegisteredTowerCount()
	{
		return registeredTowers.Count;
	}

	public static HashSet<TowerUnit> GetRegisteredTowers()
	{
		return new HashSet<TowerUnit>(registeredTowers);
	}

	public static List<ZombieUnit> GetAliveZombies()
	{
		UpdateCachedAliveZombies();
		return new List<ZombieUnit>(cachedAliveZombies);
	}

	private static int GetGridKey(Vector3 position)
	{
		var x = Mathf.FloorToInt(position.x / gridCellSize);
		var z = Mathf.FloorToInt(position.z / gridCellSize);
		return (x << 16) | (z & 0xFFFF);
	}

	private static void GetNeighborGridKeys(Vector3 center, float radius, HashSet<int> keys)
	{
		keys.Clear();
		var cellRadius = Mathf.CeilToInt(radius / gridCellSize);
		var centerX = Mathf.FloorToInt(center.x / gridCellSize);
		var centerZ = Mathf.FloorToInt(center.z / gridCellSize);

		for(var x = centerX - cellRadius; x <= centerX + cellRadius; x++)
		{
			for(var z = centerZ - cellRadius; z <= centerZ + cellRadius; z++)
			{
				keys.Add((x << 16) | (z & 0xFFFF));
			}
		}
	}

	private static void UpdateSpatialGrid()
	{
		if(zombieManager == null)
		{
			return;
		}

		foreach(var cell in spatialGrid.Values)
		{
			cell.Clear();
		}

		foreach(var zombie in cachedAliveZombies)
		{
			if(zombie != null && (!zombie.IsDead || zombie.HasPendingDamage))
			{
				var key = GetGridKey(zombie.Position);
				if(!spatialGrid.TryGetValue(key, out var cell))
				{
					cell = new List<ZombieUnit>();
					spatialGrid[key] = cell;
				}
				cell.Add(zombie);
			}
		}
	}

	private static void UpdateCachedAliveZombies()
	{
		if(zombieManager == null || Time.frameCount == lastAliveZombieUpdateFrame)
		{
			return;
		}

		lastAliveZombieUpdateFrame = Time.frameCount;
		cachedAliveZombies.Clear();

		if(validationCacheFrame != Time.frameCount)
		{
			zombieValidationCache.Clear();
			validationCacheFrame = Time.frameCount;
		}

		var allZombies = zombieManager.GetAllZombies();
		for(var i = 0; i < allZombies.Count; i++)
		{
			var zombie = allZombies[i];
			if(zombie != null && (!zombie.IsDead || zombie.HasPendingDamage))
			{
				cachedAliveZombies.Add(zombie);
				zombieValidationCache[zombie] = Time.frameCount;
			}
		}

		UpdateSpatialGrid();
	}

	private static bool IsZombieValidCached(ZombieUnit zombie)
	{
		if(zombie == null)
		{
			return false;
		}

		if(zombieValidationCache.TryGetValue(zombie, out var lastValidFrame))
		{
			return lastValidFrame == Time.frameCount && !zombie.IsDead;
		}

		var isValid = !zombie.IsDead;
		if(isValid)
		{
			zombieValidationCache[zombie] = Time.frameCount;
		}
		return isValid;
	}

	private static bool IsZombieValidForTower(ZombieUnit zombie, TowerUnit tower)
	{
		if(zombie == null)
		{
			return false;
		}

		if(zombie.IsDead || zombie.WouldDieFromDelayedDamage())
		{
			return false;
		}

		if(IsZombieTargeted(zombie))
		{
			return false;
		}

		return true;
	}

	private static void SetTowerTarget(TowerUnit tower, ZombieUnit zombie)
	{
		if(tower == null || zombie == null)
		{
			return;
		}

		ClearTowerTarget(tower);

		towerTargets[tower] = zombie;

		zombieTargeters[zombie] = tower;
	}
}
