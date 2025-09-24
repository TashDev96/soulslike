using System.Collections.Generic;
using UnityEngine;

namespace experiments
{
	public class OptimizedCapsuleCollidersManager
	{
		private static OptimizedCapsuleCollidersManager _instance;
		public static OptimizedCapsuleCollidersManager Instance
		{
			get
			{
				if (_instance == null)
					_instance = new OptimizedCapsuleCollidersManager();
				return _instance;
			}
		}
		
		private readonly List<OptimizedCapsuleCollider> _colliders = new List<OptimizedCapsuleCollider>();
		
		private OptimizedCapsuleCollidersManager() { }
		
		public OptimizedCapsuleCollider CreateCollider(Vector3 center, float height, float radius)
		{
			var collider = new OptimizedCapsuleCollider(center, height, radius);
			_colliders.Add(collider);
			
			PropagateCollidersToAll();
			return collider;
		}
		
		public bool DeleteCollider(OptimizedCapsuleCollider collider)
		{
			if (collider == null || !_colliders.Contains(collider))
				return false;
			
			_colliders.Remove(collider);
			
			PropagateCollidersToAll();
			return true;
		}
		
		public void DeleteAllColliders()
		{
			_colliders.Clear();
		}
		
		public IReadOnlyList<OptimizedCapsuleCollider> GetAllColliders()
		{
			return _colliders.AsReadOnly();
		}
		
		
		public int ColliderCount => _colliders.Count;
		
		private void PropagateCollidersToAll()
		{
			foreach (var collider in _colliders)
			{
				collider.SetAllColliders(_colliders.AsReadOnly());
			}
		}
		
		public List<OptimizedCapsuleCollider> GetCollidersInBounds(Bounds bounds)
		{
			var result = new List<OptimizedCapsuleCollider>();
			foreach (var collider in _colliders)
			{
				if (bounds.Intersects(collider.GetBounds()))
				{
					result.Add(collider);
				}
			}
			return result;
		}
		
		public List<OptimizedCapsuleCollider> GetCollidersIntersecting(OptimizedCapsuleCollider testCollider)
		{
			var result = new List<OptimizedCapsuleCollider>();
			foreach (var collider in _colliders)
			{
				if (collider != testCollider && collider.IntersectsCapsule(testCollider))
				{
					result.Add(collider);
				}
			}
			return result;
		}
	}
}
