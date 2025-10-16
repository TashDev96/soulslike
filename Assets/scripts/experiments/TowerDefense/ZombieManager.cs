using System;
using System.Collections.Generic;
using System.Linq;
using experiments;
using game.gameplay_core.characters.ai.utility.considerations.utils;
using RVO;
using SkinnedMeshInstancing;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerDefense
{
	[System.Serializable]
	public class ZombieConfig
	{
		[Header("Zombie Rendering")]
		public BakedMeshSequence zombieMeshSequence;
		public Material zombieMaterial;
		public int renderLayer;

		[Header("Zombie Stats")]
		public float zombieHealth = 100f;
		public int zombieHitValue = 10;
		public int zombieKillValue = 50;
	}

	public class ZombieManager : MonoBehaviour
	{
		[SerializeField]
		private ZombieConfig config;

		[Header("Spawn Settings")]
		[SerializeField]
		private int maxZombies = 100;
		[SerializeField]
		private Collider spawnBoundingBox;
		[SerializeField]
		private float spawnInterval = 2f;
		[SerializeField]
		private int spawnGridSize = 10;
		[SerializeField]
		private float respawnDelay = 3f;

		[Header("Goal Settings")]
		[SerializeField]
		private Transform goalTransform;

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
		}

		public void SpawnZombie()
		{
			if(simulator == null || config.zombieMeshSequence == null || config.zombieMaterial == null)
			{
				Debug.LogError("ZombieManager: Missing required components for spawning zombies.");
				return;
			}

			if(spawnBoundingBox == null)
			{
				Debug.LogError("ZombieManager: No spawn bounding box defined. Please assign a trigger collider.");
				return;
			}

			var spawnPosition = GetNextSpawnPosition();
			var zombie = new ZombieUnit(spawnPosition, config.zombieMeshSequence, config.zombieMaterial,
				config.zombieHealth, config.zombieHitValue, config.zombieKillValue, config.renderLayer);

			zombie.Initialize(simulator);
			zombie.SetTarget(new Vector3(_goalPos.x, 0, _goalPos.y));

			zombie.OnDeath += HandleZombieDeath;

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

		private void Update()
		{
			if(simulator == null)
			{
				return;
			}

			UpdateGoal();
			UpdateZombies();
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
			

			if(Time.time >= nextSpawnTime && zombies.Count < maxZombies)
			{
				SpawnZombie();
				nextSpawnTime = Time.time + spawnInterval * ZombieDensityOverTime.Evaluate(Time.time);
			}
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
			var goal = goalTransform.position;
			return new float2(goal.x, goal.z);
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
				zombie.Cleanup(simulator);
				zombie.OnDeath -= HandleZombieDeath;
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
			simulator.Dispose();
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
