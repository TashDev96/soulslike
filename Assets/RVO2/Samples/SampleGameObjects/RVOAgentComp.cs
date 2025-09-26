// -----------------------------------------------------------------------
// <copyright file="RVOAgentComp.cs" company="AillieoTech">
// Copyright (c) AillieoTech. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Game.Features.MiningPoints.LandCleanup.Harvesters;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RVO
{
	internal class RVOAgentComp : MonoBehaviour
	{
		private const float PathRecalculationInterval = 3f;

		[SerializeField]
		private Vector3 targetPosition = Vector3.zero;

		[SerializeField]
		private float gravityForce = 9.81f;
		[SerializeField]
		private float groundCheckDistance = 2f;
		[SerializeField]
		private float groundCheckOffset = 0.1f;
		[SerializeField]
		private LayerMask groundLayerMask = -1;
		private float capsuleHeight = 2f;
		private static readonly float stoppingDistance = 5f;

		private int agentId;
		private AiNavigationModule navigationModule;
		private float pathRecalculationTimer;
		private Vector3 _initialPos;

		private float verticalVelocity;
		private bool isGrounded;

		private void OnEnable()
		{
			var simulator = SampleGameObjects.GetSimulator();
			var position = new float2(transform.position.x, transform.position.z);
			agentId = simulator.AddAgent(position);

			var radius = 0.5f * transform.localScale.x;
			capsuleHeight = 2f * transform.localScale.y;
			simulator.SetAgentRadius(agentId, radius);
			simulator.SetAgentMaxSpeed(agentId, .5f);
			simulator.SetAgentTimeHorizonObst(agentId, radius * 4f);

			navigationModule = new AiNavigationModule();
			pathRecalculationTimer = Random.value;

			_initialPos = transform.position;
		}

		private void OnDisable()
		{
			var simulator = SampleGameObjects.GetSimulator();
			if(simulator == null)
			{
				return;
			}

			simulator.RemoveAgent(agentId);
			agentId = default;
		}
		
		private void Update()
		{
			var simulator = SampleGameObjects.GetSimulator();

			pathRecalculationTimer += Time.deltaTime;
			var goal = SampleGameObjects.GetGoal();
			SetPreferredVelocities(goal);

			targetPosition = new Vector3(goal.x, 0, goal.y);

			if(pathRecalculationTimer >= PathRecalculationInterval)
			{
				pathRecalculationTimer = 0f;
				if(targetPosition != Vector3.zero)
				{
					navigationModule.BuildPath(transform.position, targetPosition);

					//navigationModule.DrawDebug(Color.red, 1f);
				}
			}

			if(navigationModule.HasPath && isGrounded)
			{
				SetPreferredVelocityFromNavMesh();
			}

			var position2 = simulator.GetAgentPosition(agentId);
			if(math.lengthsq(position2 - goal) < stoppingDistance * stoppingDistance)
			{
				simulator.SetAgentPosition(agentId, new float2(_initialPos.x, _initialPos.z));
				transform.position = _initialPos;
				navigationModule.BuildPath(transform.position,targetPosition);
				return;
			}

			var prevPosition = transform.position;
			float groundY;
			var wasGrounded = isGrounded;
			isGrounded = CheckGroundAndGetPosition(out groundY);

			if(isGrounded)
			{
				if(!wasGrounded)
				{
					navigationModule.BuildPath(transform.position,targetPosition);
				}
				if(transform.position.y > groundY + 0.01f)
				{
					verticalVelocity -= gravityForce * Time.deltaTime;
				}
				else
				{
					verticalVelocity = 0f;
					transform.position = new Vector3(position2.x, groundY, position2.y);
					return;
				}
			}
			else
			{
				verticalVelocity -= gravityForce * Time.deltaTime;
			}

			var newY = prevPosition.y + verticalVelocity * Time.deltaTime;
			if(isGrounded && newY <= groundY)
			{
				newY = groundY;
				verticalVelocity = 0f;
			}

			transform.position = new Vector3(position2.x, newY, position2.y);
		}

		public void SetTarget(Vector3 target)
		{
			targetPosition = target;
			navigationModule.BuildPath(transform.position, target);
		}

		private bool CheckGroundAndGetPosition(out float groundY)
		{
			var rayStart = transform.position + Vector3.up * (capsuleHeight * 0.5f + groundCheckOffset);
			var ray = new Ray(rayStart, Vector3.down);

			if(Physics.Raycast(ray, out var hit, groundCheckDistance + capsuleHeight * 0.5f + groundCheckOffset, groundLayerMask))
			{
				groundY = hit.point.y + capsuleHeight * 0.5f;
				return true;
			}

			groundY = 0f;
			return false;
		}

		private void SetPreferredVelocityFromNavMesh()
		{
			var simulator = SampleGameObjects.GetSimulator();
			var currentPosition = transform.position;

			var moveDirection = navigationModule.CalculateMoveDirection(currentPosition, 1f, 1.5f);
			var distanceToTarget = Vector3.Distance(currentPosition, navigationModule.TargetPosition);

			float2 preferredVelocity;
			if(distanceToTarget > stoppingDistance)
			{
				preferredVelocity = new float2(moveDirection.x, moveDirection.z);
				preferredVelocity += (float2)Random.insideUnitCircle * 0.00001f;
			}
			else
			{
				return;
			}

			simulator.SetAgentPrefVelocity(agentId, preferredVelocity);
		}

		private void SetPreferredVelocities(float2 newGoal)
		{
			var simulator = SampleGameObjects.GetSimulator();

			var goalVector = newGoal - simulator.GetAgentPosition(agentId);

			if(math.lengthsq(goalVector) > stoppingDistance * stoppingDistance)
			{
				goalVector = math.normalize(goalVector);
				goalVector += (float2)Random.insideUnitCircle * 0.001f;
			}

			simulator.SetAgentPrefVelocity(agentId, goalVector);
		}

		

		private void OnDrawGizmos()
		{
			if(navigationModule != null)
			{
				//navigationModule.DrawPath(Color.green, 1f);
			}
 
		}
	}
}
