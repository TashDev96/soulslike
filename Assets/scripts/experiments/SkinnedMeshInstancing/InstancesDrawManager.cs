using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkinnedMeshInstancing
{
	public class InstancesDrawManager : MonoBehaviour
	{
		[SerializeField]
		private int _maxInstancesPerBatch = 128;
		[SerializeField]
		private bool _enableFrustumCulling = true;
		[SerializeField]
		private Camera _cullingCamera;

		private Dictionary<InstanceKey, List<IMeshInstanceInfo>> _instanceGroups;
		private Dictionary<InstanceKey, Matrix4x4[]> _matrixBuffers;
		private Dictionary<InstanceKey, MaterialPropertyBlock> _propertyBlocks;

		private void Awake()
		{
			_instanceGroups = new Dictionary<InstanceKey, List<IMeshInstanceInfo>>();
			_matrixBuffers = new Dictionary<InstanceKey, Matrix4x4[]>();
			_propertyBlocks = new Dictionary<InstanceKey, MaterialPropertyBlock>();

			if(_cullingCamera == null)
			{
				_cullingCamera = Camera.main;
			}
		}

		public void RegisterInstance(IMeshInstanceInfo instance)
		{
			if(instance == null)
			{
				return;
			}

			var key = new InstanceKey(instance.GetCurrentMesh(), instance.GetMaterial(), instance.GetLayer());

			if(!_instanceGroups.ContainsKey(key))
			{
				_instanceGroups[key] = new List<IMeshInstanceInfo>();
				_matrixBuffers[key] = new Matrix4x4[_maxInstancesPerBatch];
				_propertyBlocks[key] = new MaterialPropertyBlock();
			}

			_instanceGroups[key].Add(instance);
		}

		public void UnregisterInstance(IMeshInstanceInfo instance)
		{
			if(instance == null)
			{
				return;
			}

			var key = new InstanceKey(instance.GetCurrentMesh(), instance.GetMaterial(), instance.GetLayer());

			if(_instanceGroups.ContainsKey(key))
			{
				_instanceGroups[key].Remove(instance);

				if(_instanceGroups[key].Count == 0)
				{
					_instanceGroups.Remove(key);
					_matrixBuffers.Remove(key);
					_propertyBlocks.Remove(key);
				}
			}
		}

		public void ClearAllInstances()
		{
			_instanceGroups.Clear();
			_matrixBuffers.Clear();
			_propertyBlocks.Clear();
		}

		public int GetTotalInstanceCount()
		{
			var total = 0;
			foreach(var instances in _instanceGroups.Values)
			{
				total += instances.Count;
			}
			return total;
		}

		public int GetBatchCount()
		{
			return _instanceGroups.Count;
		}

		public Dictionary<InstanceKey, int> GetInstanceCounts()
		{
			var counts = new Dictionary<InstanceKey, int>();
			foreach(var kvp in _instanceGroups)
			{
				counts[kvp.Key] = kvp.Value.Count;
			}
			return counts;
		}

		private void Update()
		{
			UpdateInstances();
			DrawInstances();
		}

		private void UpdateInstances()
		{
			var deltaTime = Time.deltaTime;
			var keysToUpdate = new List<InstanceKey>(_instanceGroups.Keys);

			foreach(var key in keysToUpdate)
			{
				var instances = _instanceGroups[key];
				var instancesToRemove = new List<IMeshInstanceInfo>();

				foreach(var instance in instances)
				{
					if(instance == null)
					{
						instancesToRemove.Add(instance);
						continue;
					}

					instance.UpdateInstance(deltaTime);

					var newKey = new InstanceKey(instance.GetCurrentMesh(), instance.GetMaterial(), instance.GetLayer());
					if(!newKey.Equals(key))
					{
						instancesToRemove.Add(instance);
						RegisterInstance(instance);
					}
				}

				foreach(var instanceToRemove in instancesToRemove)
				{
					instances.Remove(instanceToRemove);
				}

				if(instances.Count == 0)
				{
					_instanceGroups.Remove(key);
					_matrixBuffers.Remove(key);
					_propertyBlocks.Remove(key);
				}
			}
		}

		private void DrawInstances()
		{
			foreach(var kvp in _instanceGroups)
			{
				var key = kvp.Key;
				var instances = kvp.Value;

				if(key.mesh == null || key.material == null || instances.Count == 0)
				{
					continue;
				}

				var visibleInstances = new List<IMeshInstanceInfo>();

				foreach(var instance in instances)
				{
					if(instance != null && instance.IsVisible())
					{
						if(!_enableFrustumCulling || IsInCameraFrustum(instance))
						{
							visibleInstances.Add(instance);
						}
					}
				}

				if(visibleInstances.Count == 0)
				{
					continue;
				}

				DrawInstanceBatches(key, visibleInstances);
			}
		}

		private void DrawInstanceBatches(InstanceKey key, List<IMeshInstanceInfo> instances)
		{
			var matrixBuffer = _matrixBuffers[key];
			var propertyBlock = _propertyBlocks[key];

			for(var i = 0; i < instances.Count; i += _maxInstancesPerBatch)
			{
				var batchSize = Mathf.Min(_maxInstancesPerBatch, instances.Count - i);

				for(var j = 0; j < batchSize; j++)
				{
					matrixBuffer[j] = instances[i + j].GetTransformMatrix();
				}

				Graphics.DrawMeshInstanced(
					key.mesh,
					0,
					key.material,
					matrixBuffer,
					batchSize,
					propertyBlock,
					ShadowCastingMode.On,
					true,
					key.layer,
					_cullingCamera
				);
			}
		}

		private bool IsInCameraFrustum(IMeshInstanceInfo instance)
		{
			if(_cullingCamera == null)
			{
				return true;
			}

			var bounds = instance.GetCurrentMesh()?.bounds ?? new Bounds();
			var matrix = instance.GetTransformMatrix();

			var center = matrix.MultiplyPoint3x4(bounds.center);
			var size = matrix.lossyScale;
			var maxScale = Mathf.Max(size.x, size.y, size.z);
			var radius = bounds.size.magnitude * 0.5f * maxScale;

			var planes = GeometryUtility.CalculateFrustumPlanes(_cullingCamera);
			var testBounds = new Bounds(center, Vector3.one * radius * 2f);

			return GeometryUtility.TestPlanesAABB(planes, testBounds);
		}

		public struct InstanceKey
		{
			public Mesh mesh;
			public Material material;
			public int layer;

			public InstanceKey(Mesh mesh, Material material, int layer)
			{
				this.mesh = mesh;
				this.material = material;
				this.layer = layer;
			}

			public override bool Equals(object obj)
			{
				if(obj is InstanceKey other)
				{
					return mesh == other.mesh && material == other.material && layer == other.layer;
				}
				return false;
			}

			public override int GetHashCode()
			{
				return (mesh?.GetHashCode() ?? 0) ^ (material?.GetHashCode() ?? 0) ^ layer.GetHashCode();
			}
		}
	}
}
