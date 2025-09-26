using System.Collections.Generic;
using System.Linq;
using SkinnedMeshInstancing;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using Unity.Mathematics;
using UnityEngine;
using RVO;

namespace TowerDefense
{
    public class ZombieManager : MonoBehaviour
    {
        [Header("Zombie Configuration")]
        [SerializeField] private int maxZombies = 100;
        [SerializeField] private BakedMeshSequence zombieMeshSequence;
        [SerializeField] private Material zombieMaterial;
        [SerializeField] private int renderLayer = 0;
        
        [Header("Spawn Settings")]
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private int zombiesPerWave = 10;
        
        [Header("Zombie Stats")]
        [SerializeField] private float zombieHealth = 100f;
        [SerializeField] private int zombieHitValue = 10;
        [SerializeField] private int zombieKillValue = 50;
        
        [Header("Goal Settings")]
        [SerializeField] private Transform goalTransform;
        [SerializeField] private Vector3 goalPosition = new Vector3(10f, 0f, 10f);

        private List<ZombieUnit> zombies = new List<ZombieUnit>();
        private Simulator simulator;
        private IMeshInstanceRenderer meshRenderer;
        private float2 _goalPos;
        private float lastSpawnTime;
        private int currentWave = 1;
        private int zombiesSpawnedThisWave = 0;

        public System.Action<ZombieUnit> OnZombieDied;
        public System.Action<ZombieUnit> OnZombieReachedGoal;

        private void Start()
        {
            InitializeSimulator();
            InitializeMeshRenderer();
        }

        private void InitializeSimulator()
        {
            simulator = SampleGameObjects.GetSimulator();
            if (simulator == null)
            {
                Debug.LogError("ZombieManager: Could not get RVO Simulator. Make sure SampleGameObjects is properly initialized.");
            }
        }

        private void InitializeMeshRenderer()
        {
            meshRenderer = FindFirstObjectByType<MeshInstanceRenderer>();
            if (meshRenderer == null)
            {
                var rendererGO = new GameObject("ZombieMeshInstanceRenderer");
                rendererGO.transform.SetParent(transform);
                meshRenderer = rendererGO.AddComponent<MeshInstanceRenderer>();
            }
        }

        private void Update()
        {
            if (simulator == null) return;

            UpdateGoal();
            UpdateZombies();
            HandleSpawning();
            RenderZombies();
        }

        private void UpdateGoal()
        {
            var newGoal = GetCurrentGoal();
            if (math.lengthsq(newGoal - _goalPos) > 0.1f)
            {
                _goalPos = newGoal;
                foreach (var zombie in zombies)
                {
                    if (zombie.IsInitialized)
                    {
                        zombie.SetTarget(new Vector3(_goalPos.x, 0, _goalPos.y));
                    }
                }
            }
        }

        private void UpdateZombies()
        {
            var deltaTime = Time.deltaTime;
            
            for (int i = zombies.Count - 1; i >= 0; i--)
            {
                var zombie = zombies[i];
                
                if (zombie.IsDead)
                {
                    RemoveZombie(i);
                    continue;
                }
                
                if (zombie.IsInitialized)
                {
                    zombie.UpdateAgent(simulator, _goalPos, deltaTime);
                    zombie.UpdateInstance(deltaTime);
                    
                    var distanceToGoal = Vector3.Distance(zombie.Position, new Vector3(_goalPos.x, 0, _goalPos.y));
                    if (distanceToGoal < 2f)
                    {
                        OnZombieReachedGoal?.Invoke(zombie);
                        RemoveZombie(i);
                    }
                }
            }
        }

        private void HandleSpawning()
        {
            if (Time.time >= lastSpawnTime + spawnInterval && 
                zombiesSpawnedThisWave < zombiesPerWave && 
                zombies.Count < maxZombies)
            {
                SpawnZombie();
                lastSpawnTime = Time.time;
                zombiesSpawnedThisWave++;
                
                if (zombiesSpawnedThisWave >= zombiesPerWave)
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
            if (meshRenderer != null)
            {
                var aliveZombies = zombies.Where(z => !z.IsDead).Cast<IMeshInstanceInfo>().ToList();
                meshRenderer.RenderInstances(aliveZombies);
            }
        }

        private float2 GetCurrentGoal()
        {
            Vector3 goal = goalTransform != null ? goalTransform.position : goalPosition;
            return new float2(goal.x, goal.z);
        }

        public void SpawnZombie()
        {
            if (simulator == null || zombieMeshSequence == null || zombieMaterial == null)
            {
                Debug.LogError("ZombieManager: Missing required components for spawning zombies.");
                return;
            }

            if (spawnPoints == null || spawnPoints.Count == 0)
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

        private void HandleZombieDeath(ZombieUnit zombie)
        {
            OnZombieDied?.Invoke(zombie);
        }

        private void RemoveZombie(int index)
        {
            if (index >= 0 && index < zombies.Count)
            {
                var zombie = zombies[index];
                zombie.Cleanup(simulator);
                zombie.OnDeath -= HandleZombieDeath;
                zombies.RemoveAt(index);
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                return Vector3.zero;
            }

            var randomIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
            var spawnPoint = spawnPoints[randomIndex];
            
            if (spawnPoint == null)
            {
                Debug.LogWarning($"ZombieManager: Spawn point at index {randomIndex} is null.");
                return Vector3.zero;
            }

            return spawnPoint.position;
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

        private void OnDestroy()
        {
            foreach (var zombie in zombies)
            {
                zombie.Cleanup(simulator);
            }
            zombies.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (spawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (var spawnPoint in spawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    }
                }
            }
            
            Vector3 goal = goalTransform != null ? goalTransform.position : goalPosition;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(goal, 1f);
        }
    }
}
