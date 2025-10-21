using System;
using System.Collections.Generic;
using System.Linq;
using experiments;
using game.gameplay_core.characters.ai.utility.considerations.utils;
using RVO;
using Sirenix.OdinInspector;
using SkinnedMeshInstancing;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerDefense
{
	[Serializable]
	public struct ZombiesGrade
	{
		public int health;
		public int triggerDamage;
	}

	[Serializable]
	public class ZombieConfig
	{
		[Header("Zombie Rendering")]
		public BakedMeshSequence zombieMeshSequence;
		public List<Material> zombieGradeMaterials = new();
		public int renderLayer;

		[Header("Zombie Stats")]
		public List<ZombiesGrade> grades = new();
		public float zombieScale = 1f;
	}

	public class ZombieManager : MonoBehaviour
	{
		[SerializeField]
		public ZombieConfig config;

		[Header("Spawn Settings")]
		[SerializeField]
		private int maxZombies = 100;
		[SerializeField]
		private Collider spawnBoundingBox;

		[SerializeField]
		private int spawnGridSize = 10;

		[Header("Adaptive Spawn Settings")]
		[SerializeField]
		private float spawnRateBalanceFactor = 1.0f;
		[SerializeField]
		private float minSpawnInterval = 0.1f;
		[SerializeField]
		private float maxSpawnInterval = 10f;
		[SerializeField]
		private float noTowerSpawnInterval = 1f;

		[Header("Spawn Cooling Settings")]
		[SerializeField]
		private int zombiesNearWallThreshold = 5;
		[SerializeField]
		private float spawnCoolingMultiplier = 3f;
		[SerializeField]
		private float coolingTransitionDuration = 2f;
		[SerializeField]
		private float zeroZombiesRequiredDuration = 10f;

		[Header("Grade Transition")]
		[SerializeField]
		private float gradeTransitionDuration = 30f;

		[Header("Goal Settings")]
		[SerializeField]
		private Transform goalTransform;

		[SerializeField]
		private int _currentGrade;

		public PerlinConfig ZombieDensityOverTime;

		public Action<ZombieUnit> OnZombieDied;
		public Action<ZombieUnit> OnZombieReachedGoal;

		private readonly List<ZombieUnit> zombies = new();
		private Simulator simulator;
		private IMeshInstanceRenderer meshRenderer;
		private float2 _goalPos;
		private float nextSpawnTime;
		private int currentSpawnIndex;
		private List<int> shuffledSpawnIndices;
		private int _previousGrade;
		private float _gradeTransitionStartTime;
		private bool _isTransitioning;
		private float _previousGradeHealth;
		private float _currentGradeHealth;

		private bool _isSpawnCooling;
		private float _currentCoolingFactor = 1f;
		private float _coolingTransitionStartTime;
		private float _coolingTransitionTarget = 1f;
		private float _zeroZombiesStartTime;
		private bool _hasZeroZombiesNearWall;

		private void InitializeSimulator()
		{
			simulator = SampleGameObjects.GetSimulator();
			if(simulator == null)
			{
				Debug.LogError("ZombieManager: Could not get RVO Simulator. Make sure SampleGameObjects is properly initialized.");
			}
		}

		private void InitializeMeshRenderer()
		{
			meshRenderer = FindFirstObjectByType<MeshInstanceRenderer>();
			if(meshRenderer == null)
			{
				var rendererGO = new GameObject("ZombieMeshInstanceRenderer");
				rendererGO.transform.SetParent(transform);
				meshRenderer = rendererGO.AddComponent<MeshInstanceRenderer>();
			}
		}

		private void InitializeSpawnOrder()
		{
			var totalPositions = spawnGridSize * spawnGridSize;
			shuffledSpawnIndices = new List<int>();

			for(var i = 0; i < totalPositions; i++)
			{
				shuffledSpawnIndices.Add(i);
			}

			for(var i = 0; i < shuffledSpawnIndices.Count; i++)
			{
				var temp = shuffledSpawnIndices[i];
				var randomIndex = Random.Range(i, shuffledSpawnIndices.Count);
				shuffledSpawnIndices[i] = shuffledSpawnIndices[randomIndex];
				shuffledSpawnIndices[randomIndex] = temp;
			}
		}

		private void Start()
		{
			InitializeSimulator();
			InitializeMeshRenderer();
			InitializeSpawnOrder();
			UpdateGoal();

			_currentGrade = 0;
			_previousGrade = 0;
			_currentGradeHealth = GetHealthForGrade(0);
			_previousGradeHealth = _currentGradeHealth;
			_isTransitioning = false;

			ResetSpawnTiming();
		}

		public void ResetSpawnTiming()
		{
			nextSpawnTime = Time.time;
			_gradeTransitionStartTime = Time.time;
			_coolingTransitionStartTime = Time.time;
			_zeroZombiesStartTime = Time.time;
			_isSpawnCooling = false;
			_currentCoolingFactor = 1f;
			_coolingTransitionTarget = 1f;
			_hasZeroZombiesNearWall = false;
		}

		public void OnTowerBuilt()
		{
			UpdateGradeBasedOnMaxDamage();
		}

		public void OnTowerUpgraded()
		{
			UpdateGradeBasedOnMaxDamage();
		}

		public void SpawnZombie()
		{
			if(simulator == null || config.zombieMeshSequence == null)
			{
				Debug.LogError("ZombieManager: Missing required components for spawning zombies.");
				return;
			}

			if(spawnBoundingBox == null)
			{
				Debug.LogError("ZombieManager: No spawn bounding box defined. Please assign a trigger collider.");
				return;
			}

			var grade = GetGradeForNewZombie();
			var material = GetMaterialForGrade(grade);
			var health = GetHealthForGrade(grade);

			var spawnPosition = GetNextSpawnPosition();
			var zombie = new ZombieUnit(spawnPosition, config.zombieMeshSequence, material,
				health, config.renderLayer, config.zombieScale);

			zombie.Initialize(simulator);
			zombie.SetTarget(new Vector3(_goalPos.x, 0, _goalPos.y));
			zombie.OnTakeDamage += OnZombieTookDamage;

			zombies.Add(zombie);
		}

		public List<ZombieUnit> GetAliveZombies()
		{
			return zombies.Where(z => !z.IsDead).ToList();
		}

		public List<ZombieUnit> GetAllZombies()
		{
			return new List<ZombieUnit>(zombies);
		}

		public int GetZombieCount()
		{
			return zombies.Count;
		}

		public int GetAliveZombieCount()
		{
			return zombies.Count(z => !z.IsDead);
		}

		[Button("Set Zombie Scale")]
		public void SetZombieScale(float scale)
		{
			scale = Mathf.Max(0.1f, scale);
			var healthMultiplier = scale * scale;
			config.zombieScale = scale;

			for(var i = 0; i < config.grades.Count; i++)
			{
				var grade = config.grades[i];
				grade.health = Mathf.RoundToInt(grade.health * healthMultiplier);
				config.grades[i] = grade;
			}
		}

		public float GetCurrentInterpolatedHealth()
		{
			if(!_isTransitioning)
			{
				return _currentGradeHealth;
			}

			var elapsed = Time.time - _gradeTransitionStartTime;
			var t = Mathf.Clamp01(elapsed / gradeTransitionDuration);

			if(t >= 1f)
			{
				_isTransitioning = false;
				return _currentGradeHealth;
			}

			return Mathf.Lerp(_previousGradeHealth, _currentGradeHealth, t);
		}

		private float GetMaxTowerShotDamage()
		{
			var towersManager = TowersManager.Instance;
			if(towersManager == null)
			{
				return 0f;
			}

			var towers = towersManager.GetAllTowers();
			var maxDamage = 0f;

			foreach(var tower in towers)
			{
				if(tower == null)
				{
					continue;
				}

				var config = tower.GetConfig();
				if(config == null)
				{
					continue;
				}

				var damagePerShot = tower.GetDamagePerShot();

				if(config.AttackType == AttackType.Single)
				{
					maxDamage = Mathf.Max(maxDamage, damagePerShot);
				}
				else if(config.AttackType == AttackType.Shotgun)
				{
					maxDamage = Mathf.Max(maxDamage, damagePerShot * config.BulletsPerShot);
				}
			}

			return maxDamage;
		}

		private void UpdateGradeBasedOnMaxDamage()
		{
			var maxDamage = GetMaxTowerShotDamage();
			var newGrade = GetGradeIndexForDamage(maxDamage);

			if(newGrade != _currentGrade)
			{
				_previousGrade = _currentGrade;
				_currentGrade = newGrade;
				_previousGradeHealth = _currentGradeHealth;
				_currentGradeHealth = GetHealthForGrade(_currentGrade);
				_gradeTransitionStartTime = Time.time;
				_isTransitioning = true;
			}
		}

		private int GetGradeIndexForDamage(float damage)
		{
			if(config.grades == null || config.grades.Count == 0)
			{
				return 0;
			}

			for(var i = config.grades.Count - 1; i >= 0; i--)
			{
				if(damage >= config.grades[i].triggerDamage)
				{
					return i;
				}
			}

			return 0;
		}

		private float GetHealthForGrade(int grade)
		{
			if(config.grades == null || config.grades.Count == 0)
			{
				return 100f;
			}

			grade = Mathf.Clamp(grade, 0, config.grades.Count - 1);
			return config.grades[grade].health;
		}

		private Material GetMaterialForGrade(int grade)
		{
			if(config.zombieGradeMaterials == null || config.zombieGradeMaterials.Count == 0)
			{
				return null;
			}
			try
			{
				return config.zombieGradeMaterials[grade % config.zombieGradeMaterials.Count];
			}
			catch(Exception)
			{
				Debug.LogError(grade);
				throw;
			}
		}

		private int GetGradeForNewZombie()
		{
			if(!_isTransitioning)
			{
				return _currentGrade;
			}

			var elapsed = Time.time - _gradeTransitionStartTime;
			var t = Mathf.Clamp01(elapsed / gradeTransitionDuration);

			if(t >= 1f)
			{
				_isTransitioning = false;
				return _currentGrade;
			}

			return Random.value < t ? _currentGrade : _previousGrade;
		}

		private void Update()
		{
			if(simulator == null || goalTransform == null)
			{
				return;
			}

			UpdateGoal();
			UpdateZombies();
			UpdateSpawnCooling();
			HandleSpawning();
			RenderZombies();
		}

		private void UpdateGoal()
		{
			var newGoal = GetCurrentGoal();

			if(math.lengthsq(newGoal - _goalPos) > 0.1f)
			{
				_goalPos = newGoal;
				var v3Goal = new Vector3(_goalPos.x, 0, _goalPos.y);
				if(PathFindManager.Instance != null)
				{
					PathFindManager.Instance.SetTargetPosition(v3Goal);
				}
				foreach(var zombie in zombies)
				{
					if(zombie.IsInitialized)
					{
						zombie.SetTarget(v3Goal);
					}
				}
			}
		}

		private void UpdateZombies()
		{
			var deltaTime = Time.deltaTime;

			PathFindManager.Instance.ClearSecondLayer();

			for(var i = zombies.Count - 1; i >= 0; i--)
			{
				var zombie = zombies[i];

				zombie.UpdateDelayedDamage();

				if(zombie.IsDead)
				{
					HandleZombieDeath(zombie);
					RemoveZombie(i);
					continue;
				}

				if(zombie.IsInitialized)
				{
					zombie.UpdateRadialForce();
				}
			}

			for(var i = zombies.Count - 1; i >= 0; i--)
			{
				var zombie = zombies[i];

				if(zombie.IsDead)
				{
					RemoveZombie(i);
					continue;
				}

				if(zombie.IsInitialized)
				{
					zombie.UpdateAgent(simulator, _goalPos, deltaTime);
					zombie.UpdateInstance(deltaTime);

					var distanceToGoal = Vector3.Distance(zombie.Position, new Vector3(_goalPos.x, 0, _goalPos.y));
					if(distanceToGoal < 2f)
					{
						OnZombieReachedGoal?.Invoke(zombie);
						RemoveZombie(i);
					}
				}
			}
		}

		private void HandleSpawning()
		{
			while(Time.time >= nextSpawnTime && zombies.Count < maxZombies)
			{
				SpawnZombie();
				var adaptiveInterval = CalculateAdaptiveSpawnInterval();
				nextSpawnTime += adaptiveInterval;
			}
		}

		private void UpdateSpawnCooling()
		{
			var zombiesNearWallCount = GetZombiesNearWallCount();

			if(zombiesNearWallCount > zombiesNearWallThreshold)
			{
				if(!_isSpawnCooling)
				{
					_isSpawnCooling = true;
					_coolingTransitionStartTime = Time.time;
					_coolingTransitionTarget = spawnCoolingMultiplier;
				}
				_hasZeroZombiesNearWall = false;
			}

			if(_isSpawnCooling)
			{
				if(zombiesNearWallCount == 0)
				{
					if(!_hasZeroZombiesNearWall)
					{
						_hasZeroZombiesNearWall = true;
						_zeroZombiesStartTime = Time.time;
					}
					else if(Time.time - _zeroZombiesStartTime >= zeroZombiesRequiredDuration)
					{
						_isSpawnCooling = false;
						_hasZeroZombiesNearWall = false;
						_coolingTransitionStartTime = Time.time;
						_coolingTransitionTarget = 1f;
					}
				}
				else
				{
					_hasZeroZombiesNearWall = false;
				}
			}

			var elapsed = Time.time - _coolingTransitionStartTime;
			var t = Mathf.Clamp01(elapsed / coolingTransitionDuration);
			var startValue = _currentCoolingFactor;
			_currentCoolingFactor = Mathf.Lerp(startValue, _coolingTransitionTarget, t);
		}

		private int GetZombiesNearWallCount()
		{
			var count = 0;
			for(var i = 0; i < zombies.Count; i++)
			{
				if(!zombies[i].IsDead && zombies[i].IsNearTheWall)
				{
					count++;
				}
			}
			return count;
		}

		private float CalculateAdaptiveSpawnInterval()
		{
			var towersManager = TowersManager.Instance;
			if(towersManager == null)
			{
				return noTowerSpawnInterval * _currentCoolingFactor;
			}

			var correctedTotalDPS = towersManager.GetCorrectedTotalDPS();

			if(correctedTotalDPS <= 0f)
			{
				return noTowerSpawnInterval * _currentCoolingFactor;
			}

			var zombieHealth = GetCurrentInterpolatedHealth();
			var zombiesKilledPerSecond = correctedTotalDPS / zombieHealth;

			var targetSpawnInterval = spawnRateBalanceFactor / zombiesKilledPerSecond;

			var baseInterval = Mathf.Clamp(targetSpawnInterval, minSpawnInterval, maxSpawnInterval);
			return baseInterval * _currentCoolingFactor;
		}

		private void RenderZombies()
		{
			if(meshRenderer != null)
			{
				var aliveZombies = zombies.Where(z => !z.IsDead).Cast<IMeshInstanceInfo>().ToList();
				meshRenderer.RenderInstances(aliveZombies);
			}
		}

		private float2 GetCurrentGoal()
		{
			if(goalTransform == null)
			{
				return _goalPos;
			}
			var goal = goalTransform.position;
			return new float2(goal.x, goal.z);
		}

		private void OnZombieTookDamage(ZombieUnit zombie, float damage)
		{
			if(HpBarManager.Instance != null && !zombie.IsDead)
			{
				HpBarManager.Instance.ShowHpBar(zombie);
			}
		}

		private void HandleZombieDeath(ZombieUnit zombie)
		{
			TargetingManager.OnZombieDied(zombie);
			OnZombieDied?.Invoke(zombie);
		}

		private void RemoveZombie(int index)
		{
			if(index >= 0 && index < zombies.Count)
			{
				var zombie = zombies[index];
				zombie.OnTakeDamage -= OnZombieTookDamage;
				zombie.Cleanup(simulator);
				zombies.RemoveAt(index);
			}
		}

		private Vector3 GetNextSpawnPosition()
		{
			if(spawnBoundingBox == null || shuffledSpawnIndices == null || shuffledSpawnIndices.Count == 0)
			{
				return Vector3.zero;
			}

			var bounds = spawnBoundingBox.bounds;
			var totalPositions = spawnGridSize * spawnGridSize;

			var shuffledIndex = shuffledSpawnIndices[currentSpawnIndex];
			var gridX = shuffledIndex % spawnGridSize;
			var gridZ = shuffledIndex / spawnGridSize;

			var normalizedX = (float)gridX / (spawnGridSize - 1);
			var normalizedZ = (float)gridZ / (spawnGridSize - 1);

			var worldX = bounds.min.x + normalizedX * bounds.size.x;
			var worldZ = bounds.min.z + normalizedZ * bounds.size.z;
			var worldY = bounds.center.y;

			currentSpawnIndex = (currentSpawnIndex + 1) % totalPositions;

			if(currentSpawnIndex == 0)
			{
				InitializeSpawnOrder();
			}

			return new Vector3(worldX, worldY, worldZ);
		}

		private void OnDestroy()
		{
			zombies.Clear();
			simulator?.Dispose();
		}

		private void OnDrawGizmosSelected()
		{
			if(spawnBoundingBox != null)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireCube(spawnBoundingBox.bounds.center, spawnBoundingBox.bounds.size);

				Gizmos.color = Color.cyan;
				var bounds = spawnBoundingBox.bounds;
				for(var x = 0; x < spawnGridSize; x++)
				{
					for(var z = 0; z < spawnGridSize; z++)
					{
						var normalizedX = (float)x / (spawnGridSize - 1);
						var normalizedZ = (float)z / (spawnGridSize - 1);

						var worldX = bounds.min.x + normalizedX * bounds.size.x;
						var worldZ = bounds.min.z + normalizedZ * bounds.size.z;
						var worldY = bounds.center.y;

						Gizmos.DrawWireSphere(new Vector3(worldX, worldY, worldZ), 0.2f);
					}
				}
			}

			if(goalTransform != null)
			{
				var goal = goalTransform.position;
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(goal, 1f);
			}
		}
	}
}
