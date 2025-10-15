using System;
using System.Collections.Generic;
using System.Linq;
using experiments;
using RVO;
using SkinnedMeshInstancing;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerDefense
{
	public class ZombieManager : MonoBehaviour
	{
		[Header("Zombie Configuration")]
		[SerializeField]
		private int maxZombies = 100;
		[SerializeField]
		private BakedMeshSequence zombieMeshSequence;
		[SerializeField]
		private Material zombieMaterial;
		[SerializeField]
		private int renderLayer;

		[Header("Spawn Settings")]
		[SerializeField]
		private List<Transform> spawnPoints = new();
		[SerializeField]
		private float spawnInterval = 2f;
		[SerializeField]
		private int zombiesPerWave = 10;

		[Header("Zombie Stats")]
		[SerializeField]
		private float zombieHealth = 100f;
		[SerializeField]
		private int zombieHitValue = 10;
		[SerializeField]
		private int zombieKillValue = 50;

		[Header("Goal Settings")]
		[SerializeField]
		private Transform goalTransform;

		public Action<ZombieUnit> OnZombieDied;
		public Action<ZombieUnit> OnZombieReachedGoal;

		private readonly List<ZombieUnit> zombies = new();
		private Simulator simulator;
		private IMeshInstanceRenderer meshRenderer;
		private float2 _goalPos;
		private float lastSpawnTime;
		private int currentWave = 1;
		private int zombiesSpawnedThisWave;

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

		private void Start()
		{
			InitializeSimulator();
			InitializeMeshRenderer();
			UpdateGoal();
		}

		public void SpawnZombie()
		{
			if(simulator == null || zombieMeshSequence == null || zombieMaterial == null)
			{
				Debug.LogError("ZombieManager: Missing required components for spawning zombies.");
				return;
			}

			if(spawnPoints == null || spawnPoints.Count == 0)
			{
				Debug.LogError("ZombieManager: No spawn points defined. Please assign spawn point transforms.");
				return;
			}

			var spawnPosition = GetRandomSpawnPosition();
			var zombie = new ZombieUnit(spawnPosition, zombieMeshSequence, zombieMaterial,
				zombieHealth, zombieHitValue, zombieKillValue, renderLayer);

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

		public int GetCurrentWave()
		{
			return currentWave;
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
			if(Time.time >= lastSpawnTime + spawnInterval &&
			   zombiesSpawnedThisWave < zombiesPerWave &&
			   zombies.Count < maxZombies)
			{
				SpawnZombie();
				lastSpawnTime = Time.time;
				zombiesSpawnedThisWave++;

				if(zombiesSpawnedThisWave >= zombiesPerWave)
				{
					StartNextWave();
				}
			}
		}

		private void StartNextWave()
		{
			currentWave++;
			zombiesSpawnedThisWave = 0;

			zombieHealth += 20f;
			zombieHitValue += 2;
			zombieKillValue += 10;

			Debug.Log($"Wave {currentWave} started! Zombie health: {zombieHealth}");
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

		private Vector3 GetRandomSpawnPosition()
		{
			if(spawnPoints == null || spawnPoints.Count == 0)
			{
				return Vector3.zero;
			}

			var randomIndex = Random.Range(0, spawnPoints.Count);
			var spawnPoint = spawnPoints[randomIndex];

			if(spawnPoint == null)
			{
				Debug.LogWarning($"ZombieManager: Spawn point at index {randomIndex} is null.");
				return Vector3.zero;
			}

			return spawnPoint.position;
		}

		private void OnDestroy()
		{
			zombies.Clear();
			simulator.Dispose();
		}

		private void OnDrawGizmosSelected()
		{
			if(spawnPoints != null)
			{
				Gizmos.color = Color.blue;
				foreach(var spawnPoint in spawnPoints)
				{
					if(spawnPoint != null)
					{
						Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
					}
				}
			}

			var goal = goalTransform.position;
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(goal, 1f);
		}
	}
}
