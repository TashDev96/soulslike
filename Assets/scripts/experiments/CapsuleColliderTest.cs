using System.Collections.Generic;
using dream_lib.src.utils.drawers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace experiments
{
	public class CapsuleColliderTest : MonoBehaviour
	{
		[SerializeField]
		private int spawnCount = 10;
		[SerializeField]
		private float minSpeed = 1f;
		[SerializeField]
		private float maxSpeed = 5f;
		[SerializeField]
		private float minHeight = 1f;
		[SerializeField]
		private float maxHeight = 3f;
		[SerializeField]
		private float minRadius = 0.2f;
		[SerializeField]
		private float maxRadius = 0.8f;
		[SerializeField]
		private Vector3 gravity = new(0f, -9.81f, 0f);
		[SerializeField]
		private Vector3 spawnArea = new(20f, 10f, 20f);
		[SerializeField]
		private float spawnHeight = 10f;
		[SerializeField]
		private bool showDebugInfo = true;
		[SerializeField]
		private int layerMask = Physics.DefaultRaycastLayers;
		[SerializeField]
		private float skinWidth = 0.05f;
		[SerializeField]
		private float groundSnapDistance = 0.1f;
		[SerializeField]
		private float minGroundAngle = 0.7f;

		private readonly List<MovingCapsuleCollider> _movingColliders = new();

		private void Start()
		{
			SpawnColliders();
		}

		[ContextMenu("Respawn Colliders")]
		public void RespawnColliders()
		{
			foreach(var movingCollider in _movingColliders)
			{
				movingCollider.Destroy();
			}
			_movingColliders.Clear();

			SpawnColliders();
		}

		[Button("Add More Colliders")]
		public void AddMoreColliders()
		{
			var oldCount = spawnCount;
			spawnCount += 5;

			for(var i = oldCount; i < spawnCount; i++)
			{
				var randomPos = transform.position + new Vector3(
					Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
					spawnHeight + Random.Range(0f, spawnArea.y),
					Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
				);

				var randomVelocity = new Vector3(
					Random.Range(-1f, 1f),
					Random.Range(-1f, 1f),
					Random.Range(-1f, 1f)
				).normalized * Random.Range(minSpeed, maxSpeed);

				var height = Random.Range(minHeight, maxHeight);
				var radius = Random.Range(minRadius, maxRadius);

				var movingCollider = new MovingCapsuleCollider(randomPos, height, radius, randomVelocity);
				movingCollider.LayerMask = layerMask;
				movingCollider.SkinWidth = skinWidth;
				movingCollider.GroundSnapDistance = groundSnapDistance;
				movingCollider.MinGroundAngle = minGroundAngle;
				_movingColliders.Add(movingCollider);
			}
		}

		private void SpawnColliders()
		{
			for(var i = 0; i < spawnCount; i++)
			{
				var randomPos = transform.position + new Vector3(
					Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
					spawnHeight + Random.Range(0f, spawnArea.y),
					Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
				);

				var randomVelocity = new Vector3(
					Random.Range(-1f, 1f),
					Random.Range(-1f, 1f),
					Random.Range(-1f, 1f)
				).normalized * Random.Range(minSpeed, maxSpeed);

				var height = Random.Range(minHeight, maxHeight);
				var radius = Random.Range(minRadius, maxRadius);

				var movingCollider = new MovingCapsuleCollider(randomPos, height, radius, randomVelocity);
				movingCollider.LayerMask = layerMask;
				movingCollider.SkinWidth = skinWidth;
				movingCollider.GroundSnapDistance = groundSnapDistance;
				movingCollider.MinGroundAngle = minGroundAngle;
				_movingColliders.Add(movingCollider);
			}
		}

		private void Update()
		{
			UpdateColliders();
		}

		private void UpdateColliders()
		{
			foreach(var movingCollider in _movingColliders)
			{
				movingCollider.Collider.SkipCollisions = false;
				movingCollider.Collider.DontCollideWith.Clear();
			}

			foreach(var movingCollider in _movingColliders)
			{
				if(!movingCollider.IsActive)
				{
					continue;
				}

				movingCollider.Update(Time.deltaTime, gravity);
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(transform.position + Vector3.up * spawnHeight, spawnArea);

			foreach(var movingCollider in _movingColliders)
			{
				if(!movingCollider.IsActive)
				{
					continue;
				}

				var collider = movingCollider.Collider;
				var color = movingCollider.DebugColor;

				var intersecting = collider.GetIntersectingColliders();
				if(collider.SkipCollisions)
				{
					color = Color.red;
				}

				var isGrounded = movingCollider.IsGrounded();
				if(isGrounded)
				{
					color = Color.Lerp(color, Color.green, 0.3f);
				}

				DebugDrawUtils.DrawWireCapsule(collider.point1, collider.point2, collider.Radius, color);

				if(showDebugInfo)
				{
					var velocity = movingCollider.Velocity;
					Debug.DrawLine(collider.Center, collider.Center + velocity.normalized * 2f, Color.yellow);

					var statusText = $"V:{velocity.magnitude:F1}";
					if(isGrounded) statusText += " [G]";
					DebugDrawUtils.DrawText(statusText, collider.Center + Vector3.up * (collider.Height * 0.5f + 0.5f), 0f);
				}
			}
		}

		private void OnDestroy()
		{
			foreach(var movingCollider in _movingColliders)
			{
				movingCollider.Destroy();
			}
			_movingColliders.Clear();
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(transform.position + Vector3.up * spawnHeight, spawnArea);
			
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, transform.position + gravity.normalized * 5f);
		}
	}
}