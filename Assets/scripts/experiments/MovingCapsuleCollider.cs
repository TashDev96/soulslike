using UnityEngine;

namespace experiments
{
	public class MovingCapsuleCollider
	{
		public OptimizedCapsuleCollider Collider { get; private set; }
		public Vector3 Velocity { get; set; }
		public Color DebugColor { get; set; }
		public bool IsActive { get; set; }
		public float Mass { get; set; } = 1f;
		public bool UseGravity { get; set; } = true;
		public int LayerMask { get; set; } = Physics.DefaultRaycastLayers;
		public float SkinWidth { get; set; } = 0.05f;
		public float GroundSnapDistance { get; set; } = 0.1f;
		public float MinGroundAngle { get; set; } = 0.7f;

		public MovingCapsuleCollider(Vector3 center, float height, float radius, Vector3 velocity)
		{
			Collider = OptimizedCapsuleCollidersManager.Instance.CreateCollider(center, height, radius);
			Velocity = velocity;
			DebugColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.8f, 1f);
			IsActive = true;
			Mass = Random.Range(0.5f, 2f);
			LayerMask = Physics.DefaultRaycastLayers;
			SkinWidth = 0.01f;
		}

		public void Update(float deltaTime, Vector3 gravity)
		{
			if(!IsActive)
			{
				return;
			}

			var wasGrounded = IsGrounded();

			if(UseGravity)
			{
				Velocity += gravity * deltaTime;
			}

			var desiredMovement = Velocity * deltaTime;
			var safeMovement = CalculateSafeMovement(desiredMovement);

			var newCenter = Collider.Center + safeMovement;
			Collider.SetCenter(newCenter);

			var isNowGrounded = IsGrounded();

			if(isNowGrounded && !wasGrounded && Velocity.y < 0)
			{
				Velocity = new Vector3(Velocity.x, 0f, Velocity.z);
			}
			else if(isNowGrounded && Velocity.y < 0)
			{
				Velocity = new Vector3(Velocity.x, 0f, Velocity.z);
			}
		}

		public void Destroy()
		{
			if(Collider != null)
			{
				OptimizedCapsuleCollidersManager.Instance.DeleteCollider(Collider);
				Collider = null;
			}
			IsActive = false;
		}

		public void ApplyForce(Vector3 force, float deltaTime)
		{
			Velocity += force / Mass * deltaTime;
		}

		public bool IsGrounded()
		{
			var castDistance = GroundSnapDistance + SkinWidth + 0.001f;
			return Collider.CapsuleCast(Vector3.down, out var hit, castDistance, LayerMask) &&
			       Vector3.Dot(hit.normal, Vector3.up) > MinGroundAngle &&
			       hit.distance <= GroundSnapDistance + SkinWidth;
		}

		private Vector3 CalculateSafeMovement(Vector3 desiredMovement)
		{
			var direction = desiredMovement.normalized;
			var distance = desiredMovement.magnitude;

			var minDist = distance;

			if(Collider.CapsuleCast(direction, out var hit, distance + SkinWidth, LayerMask))
			{
				var safeDistance = hit.distance - SkinWidth;
				if(safeDistance < 0f)
				{
					minDist = Mathf.Min(minDist, safeDistance);
				}
				else
				{
					minDist = Mathf.Min(minDist, Mathf.Max(0f, safeDistance));
				}
			}

			if(Collider.OptimizedCapsuleCast(direction, distance + SkinWidth, out var hit2))
			{
				var safeDistance = hit2.distance - SkinWidth;
				//hit2.collider.DontCollideWith.Add(Collider);
				if(safeDistance < 0f)
				{
					minDist = Mathf.Min(minDist, safeDistance);
				}
				else
				{
					minDist = Mathf.Min(minDist, Mathf.Max(0f, safeDistance));
				}
			}

			return direction*minDist;
		}

		private void HandleCollisionResponse(Vector3 desiredMovement, Vector3 safeMovement)
		{
			if(Collider.CapsuleCast(desiredMovement.normalized, out var hit, desiredMovement.magnitude + SkinWidth, LayerMask))
			{
				var normal = hit.normal;
				var dotProduct = Vector3.Dot(Velocity, normal);

				if(dotProduct < 0)
				{
					Velocity -= dotProduct * normal;

					if(Vector3.Dot(normal, Vector3.up) > MinGroundAngle)
					{
						Velocity = new Vector3(Velocity.x * 0.8f, 0f, Velocity.z * 0.8f);
					}
					else
					{
						Velocity *= 0.7f;
					}
				}
			}
		}
	}
}
