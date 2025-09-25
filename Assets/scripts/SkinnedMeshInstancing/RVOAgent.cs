using Game.Features.MiningPoints.LandCleanup.Harvesters;
using SkinnedMeshInstancing;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RVO
{
	public class RVOAgent : IMeshInstanceInfo
	{
		private const float PathRecalculationInterval = 1f;
		private const float StoppingDistance = 4f;

		private Vector3 position;
		private Vector3 targetPosition;
		private readonly float gravityForce = 9.81f;
		private readonly float groundCheckDistance = 2f;
		private readonly float groundCheckOffset = 0.1f;
		private readonly LayerMask groundLayerMask = -1;
		private readonly float capsuleHeight = 2f;
		private readonly float radius = 0.33f;
		private readonly float maxSpeed = 0.7f;
		private readonly float scale;

		private AiNavigationModule navigationModule;
		private float pathRecalculationTimer;
		private readonly Vector3 initialPosition;
		private float verticalVelocity;
		private bool isGrounded;
		private bool isVisible = true;

		private readonly BakedMeshSequence meshSequence;
		private readonly Material material;
		private readonly int layer;
		private readonly string currentAnimationClip = "Walk";
		private float animationTime;
		private float movementSpeed;
		private bool isMoving;
		private Quaternion rotation = Quaternion.identity;

		public Vector3 Position => position;
		public int AgentId { get; private set; } = -1;

		public bool IsInitialized => AgentId != -1;

		public RVOAgent(Vector3 startPosition, BakedMeshSequence meshSequence, Material material, int layer = 0, float scale = 1f)
		{
			position = startPosition;
			initialPosition = startPosition;
			this.meshSequence = meshSequence;
			this.material = material;
			this.layer = layer;
			this.scale = scale;
			targetPosition = Vector3.zero;
			animationTime = Random.value * meshSequence.GetClipDuration(currentAnimationClip);
			movementSpeed = 0f;
			isMoving = false;
		}

		public void Initialize(Simulator simulator)
		{
			if(AgentId != -1)
			{
				return;
			}

			var position2D = new float2(position.x, position.z);
			AgentId = simulator.AddAgent(position2D);

			simulator.SetAgentRadius(AgentId, radius * scale);
			simulator.SetAgentMaxSpeed(AgentId, maxSpeed);
			simulator.SetAgentTimeHorizonObst(AgentId, 4f);

			navigationModule = new AiNavigationModule();
			pathRecalculationTimer = Random.value;
			navigationModule.BuildPath(position, targetPosition);
		}

		public void Cleanup(Simulator simulator)
		{
			if(AgentId == -1 || simulator == null)
			{
				return;
			}

			simulator.RemoveAgent(AgentId);
			AgentId = -1;
		}

		public void SetTarget(Vector3 target)
		{
			targetPosition = target;
			if(navigationModule != null)
			{
				navigationModule.BuildPath(position, target);
			}
		}

		public void UpdateAgent(Simulator simulator, float2 goal, float deltaTime)
		{
			if(AgentId == -1)
			{
				return;
			}

			pathRecalculationTimer += deltaTime;
			SetPreferredVelocities(simulator, goal);

			targetPosition = new Vector3(goal.x, 0, goal.y);
			var position2D = simulator.GetAgentPosition(AgentId);
			var distanceToTarget = math.lengthsq(position2D - goal);

			var recalculatePathDelay = Mathf.Lerp(0, 5, distanceToTarget / 20f);

			if(pathRecalculationTimer >= PathRecalculationInterval + recalculatePathDelay)
			{
				pathRecalculationTimer = 0f;
				if(targetPosition != Vector3.zero && navigationModule != null)
				{
					//navigationModule.BuildPath(position, targetPosition);
				}
			}

			if(navigationModule != null && navigationModule.HasPath && isGrounded)
			{
				SetPreferredVelocityFromNavMesh(simulator);
			}

			if(distanceToTarget < StoppingDistance * StoppingDistance)
			{
				//restart
				simulator.SetAgentPosition(AgentId, new float2(initialPosition.x, initialPosition.z));
				position = initialPosition;
				animationTime = Random.value * meshSequence.GetClipDuration(currentAnimationClip);

				if(navigationModule != null)
				{
					navigationModule.BuildPath(position, targetPosition);
				}
				return;
			}

			var prevPosition = position;
			var wasGrounded = isGrounded;
			isGrounded = CheckGroundAndGetPosition(out var groundY);

			if(isGrounded)
			{
				if(!wasGrounded && navigationModule != null)
				{
					navigationModule.BuildPath(position, targetPosition);
				}
				if(position.y > groundY + 0.01f)
				{
					verticalVelocity -= gravityForce * deltaTime;
				}
				else
				{
					verticalVelocity = 0f;
				}
			}
			else
			{
				verticalVelocity -= gravityForce * deltaTime;
			}

			var newY = prevPosition.y + verticalVelocity * deltaTime;
			if(isGrounded && newY <= groundY)
			{
				newY = groundY;
				verticalVelocity = 0f;
			}

			var newPosition = new Vector3(position2D.x, newY, position2D.y);
			var movementDelta = newPosition - position;
			movementSpeed = movementDelta.magnitude / deltaTime;
			isMoving = movementSpeed > 0.1f;

			if(isMoving)
			{
				var movementDirection = new Vector3(movementDelta.x, 0, movementDelta.z).normalized;
				rotation = Quaternion.Lerp(rotation, Quaternion.LookRotation(movementDirection, Vector3.up), deltaTime * 10f);
			}

			position = newPosition;
			
			//Debug.DrawLine(position, position+Vector3.up);
		}

		public Matrix4x4 GetTransformMatrix()
		{
			return Matrix4x4.TRS(position, rotation, Vector3.one * scale);
		}

		public Mesh GetCurrentMesh()
		{
			if(meshSequence == null)
			{
				return null;
			}

			if(isMoving)
			{
				return meshSequence.GetMeshAtTime(animationTime, currentAnimationClip);
			}
			return meshSequence.GetMeshAtFrame(0);
		}

		public Material GetMaterial()
		{
			return material;
		}

		public int GetLayer()
		{
			return layer;
		}

		public bool IsVisible()
		{
			return isVisible;
		}

		public void SetVisible(bool visible)
		{
			isVisible = visible;
		}

		public void UpdateInstance(float deltaTime)
		{
			if(isMoving && meshSequence != null)
			{
				animationTime += deltaTime / scale;
				var clipDuration = meshSequence.GetClipDuration(currentAnimationClip);
				if(clipDuration > 0 && animationTime >= clipDuration)
				{
					animationTime = 0f;
				}
			}
		}

		private bool CheckGroundAndGetPosition(out float groundY)
		{
			var rayStart = position + Vector3.up * (capsuleHeight * 0.5f + groundCheckOffset);
			var ray = new Ray(rayStart, Vector3.down);

			if(Physics.Raycast(ray, out var hit, groundCheckDistance + capsuleHeight * 0.5f + groundCheckOffset, groundLayerMask))
			{
				groundY = hit.point.y + capsuleHeight * 0.5f;
				return true;
			}

			groundY = 0f;
			return false;
		}

		private void SetPreferredVelocityFromNavMesh(Simulator simulator)
		{
			var moveDirection = navigationModule.CalculateMoveDirection(position, 1f, 1.5f);
			var distanceToTarget = Vector3.Distance(position, navigationModule.TargetPosition);

			float2 preferredVelocity;
			if(distanceToTarget > StoppingDistance)
			{
				preferredVelocity = new float2(moveDirection.x, moveDirection.z);
				preferredVelocity += (float2)Random.insideUnitCircle * 0.00001f;
			}
			else
			{
				return;
			}

			simulator.SetAgentPrefVelocity(AgentId, preferredVelocity);
		}

		private void SetPreferredVelocities(Simulator simulator, float2 newGoal)
		{
			var goalVector = newGoal - simulator.GetAgentPosition(AgentId);

			if(math.lengthsq(goalVector) > StoppingDistance * StoppingDistance)
			{
				goalVector = math.normalize(goalVector);
				goalVector += (float2)Random.insideUnitCircle * 0.001f;
			}

			simulator.SetAgentPrefVelocity(AgentId, goalVector);
		}
	}
}
