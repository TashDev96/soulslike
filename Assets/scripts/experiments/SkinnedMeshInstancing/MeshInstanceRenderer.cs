using System.Collections.Generic;
using UnityEngine;

namespace SkinnedMeshInstancing
{
    public class MeshInstanceRenderer : MonoBehaviour, IMeshInstanceRenderer
    {
        [SerializeField] private InstancesDrawManager drawManager;
        
        private HashSet<IMeshInstanceInfo> registeredInstances = new HashSet<IMeshInstanceInfo>();

        private void Awake()
        {
            if (drawManager == null)
            {
                drawManager = FindObjectOfType<InstancesDrawManager>();
                if (drawManager == null)
                {
                    var drawManagerGO = new GameObject("InstancesDrawManager");
                    drawManagerGO.transform.SetParent(transform);
                    drawManager = drawManagerGO.AddComponent<InstancesDrawManager>();
                }
            }
        }

        public void RenderInstances(IEnumerable<IMeshInstanceInfo> instances)
        {
            if (drawManager == null) return;

            var currentInstances = new HashSet<IMeshInstanceInfo>(instances);
            
            foreach (var instance in registeredInstances)
            {
                if (!currentInstances.Contains(instance))
                {
                    drawManager.UnregisterInstance(instance);
                }
            }
            
            foreach (var instance in currentInstances)
            {
                if (!registeredInstances.Contains(instance))
                {
                    drawManager.RegisterInstance(instance);
                }
            }
            
            registeredInstances = currentInstances;
        }

        public void RegisterInstance(IMeshInstanceInfo instance)
        {
            if (instance == null || drawManager == null) return;
            
            if (registeredInstances.Add(instance))
            {
                drawManager.RegisterInstance(instance);
            }
        }

        public void UnregisterInstance(IMeshInstanceInfo instance)
        {
            if (instance == null || drawManager == null) return;
            
            if (registeredInstances.Remove(instance))
            {
                drawManager.UnregisterInstance(instance);
            }
        }

        public void ClearAllInstances()
        {
            if (drawManager == null) return;
            
            drawManager.ClearAllInstances();
            registeredInstances.Clear();
        }

        public int GetTotalInstanceCount()
        {
            return drawManager?.GetTotalInstanceCount() ?? 0;
        }
    }
}
