using System.Collections.Generic;
using UnityEngine;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;

namespace SkinnedMeshInstancing
{
	public class MeshSequenceInstanceManager : MonoBehaviour
	{
		[SerializeField] private InstancesDrawManager _drawManager;
		[SerializeField] private BakedMeshSequence _defaultMeshSequence;
		[SerializeField] private Material _defaultMaterial;
		[SerializeField] private int _defaultLayer = 0;
		[SerializeField] private bool _autoRegisterOnStart = true;
		
		private List<MeshSequenceInstance> _instances = new List<MeshSequenceInstance>();
		
		private void Start()
		{
			if (_drawManager == null)
				_drawManager = FindObjectOfType<InstancesDrawManager>();
				
			if (_autoRegisterOnStart)
			{
				RegisterAllInstances();
			}
		}
		
		private void OnDestroy()
		{
			UnregisterAllInstances();
		}
		
		public MeshSequenceInstance CreateInstance(Transform transform, BakedMeshSequence meshSequence = null, Material material = null)
		{
			var sequence = meshSequence ?? _defaultMeshSequence;
			var mat = material ?? _defaultMaterial;
			
			if (sequence == null || mat == null || transform == null)
			{
				Debug.LogWarning("Cannot create instance: missing required components");
				return null;
			}
			
			var instance = new MeshSequenceInstance(transform, sequence, mat);
			instance.SetLayer(_defaultLayer);
			
			_instances.Add(instance);
			
			if (_drawManager != null)
				_drawManager.RegisterInstance(instance);
				
			return instance;
		}
		
		public void RemoveInstance(MeshSequenceInstance instance)
		{
			if (instance == null) return;
			
			_instances.Remove(instance);
			
			if (_drawManager != null)
				_drawManager.UnregisterInstance(instance);
		}
		
		public void RegisterAllInstances()
		{
			if (_drawManager == null) return;
			
			foreach (var instance in _instances)
			{
				_drawManager.RegisterInstance(instance);
			}
		}
		
		public void UnregisterAllInstances()
		{
			if (_drawManager == null) return;
			
			foreach (var instance in _instances)
			{
				_drawManager.UnregisterInstance(instance);
			}
		}
		
		public void PlayAllInstances(string clipId = null)
		{
			foreach (var instance in _instances)
			{
				instance.Play(clipId);
			}
		}
		
		public void StopAllInstances()
		{
			foreach (var instance in _instances)
			{
				instance.Stop();
			}
		}
		
		public void SetAllInstancesVisible(bool visible)
		{
			foreach (var instance in _instances)
			{
				instance.SetVisible(visible);
			}
		}
		
		public void SetAllInstancesPlaybackSpeed(float speed)
		{
			foreach (var instance in _instances)
			{
				instance.SetPlaybackSpeed(speed);
			}
		}
		
		public void ClearAllInstances()
		{
			UnregisterAllInstances();
			_instances.Clear();
		}
		
		public List<MeshSequenceInstance> GetInstances()
		{
			return new List<MeshSequenceInstance>(_instances);
		}
		
		public int GetInstanceCount()
		{
			return _instances.Count;
		}
		
		public MeshSequenceInstance[] CreateInstancesFromTransforms(Transform[] transforms, BakedMeshSequence meshSequence = null, Material material = null)
		{
			var instances = new MeshSequenceInstance[transforms.Length];
			
			for (int i = 0; i < transforms.Length; i++)
			{
				instances[i] = CreateInstance(transforms[i], meshSequence, material);
			}
			
			return instances;
		}
		
		public void SetDrawManager(InstancesDrawManager drawManager)
		{
			if (_drawManager != null)
				UnregisterAllInstances();
				
			_drawManager = drawManager;
			
			if (_drawManager != null)
				RegisterAllInstances();
		}
	}
}
