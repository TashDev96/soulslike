using System.Collections.Generic;
using SkinnedMeshInstancing;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using Unity.Mathematics;
using UnityEngine;

namespace RVO
{
    public class RVOAgentManager : MonoBehaviour
    {
        [Header("Agent Configuration")]
        [SerializeField] private int maxAgents = 100;
        [SerializeField] private BakedMeshSequence agentMeshSequence;
        [SerializeField] private Material agentMaterial;
        [SerializeField] private int renderLayer = 0;
        
        [Header("Spawn Settings")]
        [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private int initialAgentCount = 50;
        
        [Header("Goal Settings")]
        [SerializeField] private Transform goalTransform;
        [SerializeField] private Vector3 goalPosition = new Vector3(10f, 0f, 10f);
        
        private List<RVOAgent> agents = new List<RVOAgent>();
        private Simulator simulator;
        private IMeshInstanceRenderer meshRenderer;
        private float2 _goalPos;

        private void Start()
        {
            InitializeSimulator();
            InitializeMeshRenderer();
            
            if (spawnOnStart)
            {
                SpawnAgents(initialAgentCount);
            }
        }

        private void InitializeSimulator()
        {
            simulator = SampleGameObjects.GetSimulator();
            if (simulator == null)
            {
                Debug.LogError("RVOAgentManager: Could not get RVO Simulator. Make sure SampleGameObjects is properly initialized.");
            }
        }

        private void InitializeMeshRenderer()
        {
            meshRenderer = FindFirstObjectByType<MeshInstanceRenderer>();
            if (meshRenderer == null)
            {
                var rendererGO = new GameObject("MeshInstanceRenderer");
                rendererGO.transform.SetParent(transform);
                meshRenderer = rendererGO.AddComponent<MeshInstanceRenderer>();
            }
        }

        private void Update()
        {
            if (simulator == null) return;

            var newGoal = GetCurrentGoal();
            if(math.lengthsq(newGoal - _goalPos) > 0.1f)
            {
	            _goalPos = newGoal;
	            foreach(var agent in agents)
	            {
		            agent.SetTarget(new Vector3(_goalPos.x,0,_goalPos.y));
	            }
            }
            
            
            var deltaTime = Time.deltaTime;

            for (int i = 0; i < agents.Count; i++)
            {
                var agent = agents[i];
                if (agent.IsInitialized)
                {
                    agent.UpdateAgent(simulator, _goalPos, deltaTime);
                    agent.UpdateInstance(deltaTime);
                }
            }

            if (meshRenderer != null)
            {
                meshRenderer.RenderInstances(agents);
            }
        }

        private float2 GetCurrentGoal()
        {
            Vector3 goal = goalTransform != null ? goalTransform.position : goalPosition;
            return new float2(goal.x, goal.z);
        }

        public void SpawnAgents(int count)
        {
            if (simulator == null || agentMeshSequence == null || agentMaterial == null)
            {
                Debug.LogError("RVOAgentManager: Missing required components for spawning agents.");
                return;
            }

            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                Debug.LogError("RVOAgentManager: No spawn points defined. Please assign spawn point transforms.");
                return;
            }

            int agentsToSpawn = Mathf.Min(count, maxAgents - agents.Count);
            
            for (int i = 0; i < agentsToSpawn; i++)
            {
                var spawnPosition = GetRandomSpawnPosition();
                var agent = new RVOAgent(spawnPosition, agentMeshSequence, agentMaterial, renderLayer);
                agent.Initialize(simulator);
                agents.Add(agent);
            }

            Debug.Log($"RVOAgentManager: Spawned {agentsToSpawn} agents. Total agents: {agents.Count}");
        }

        public void DespawnAgents(int count)
        {
            if (simulator == null) return;

            int agentsToDespawn = Mathf.Min(count, agents.Count);
            
            for (int i = 0; i < agentsToDespawn; i++)
            {
                var lastIndex = agents.Count - 1;
                var agent = agents[lastIndex];
                agent.Cleanup(simulator);
                agents.RemoveAt(lastIndex);
            }

            Debug.Log($"RVOAgentManager: Despawned {agentsToDespawn} agents. Total agents: {agents.Count}");
        }

        public void ClearAllAgents()
        {
            if (simulator == null) return;

            foreach (var agent in agents)
            {
                agent.Cleanup(simulator);
            }
            
            agents.Clear();
            Debug.Log("RVOAgentManager: Cleared all agents.");
        }

        public void SetAgentsVisible(bool visible)
        {
            foreach (var agent in agents)
            {
                agent.SetVisible(visible);
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
                Debug.LogWarning($"RVOAgentManager: Spawn point at index {randomIndex} is null.");
                return Vector3.zero;
            }

            return spawnPoint.position;
        }

        private void OnDestroy()
        {
            ClearAllAgents();
        }

        private void OnDrawGizmosSelected()
        {
            if (spawnPoints != null)
            {
                Gizmos.color = Color.yellow;
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
            
            if (agents != null)
            {
                Gizmos.color = Color.green;
                foreach (var agent in agents)
                {
                    if (agent != null)
                    {
                        Gizmos.DrawWireSphere(agent.Position, 0.5f);
                    }
                }
            }
        }

        public int GetAgentCount()
        {
            return agents.Count;
        }

        public List<RVOAgent> GetAgents()
        {
            return new List<RVOAgent>(agents);
        }
    }
}
