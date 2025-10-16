using experiments;
using SkinnedMeshInstancing;
using SkinnedMeshInstancing.AnimationBaker.AnimationData;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RVO
{
	public class RVOAgent : IMeshInstanceInfo
	{
		private const float StoppingDistance = 4f;

		private Vector3 _position;
		private Vector3 _targetPosition;
		private readonly float _gravityForce = 9.81f;
		private readonly float _groundCheckDistance = 2f;
		private readonly float _groundCheckOffset = 0.1f;
		private static readonly LayerMask _groundLayerMask = LayerMask.GetMask("LevelGeometry");
		private readonly float _capsuleHeight = 2f;
		private readonly float _radius = 0.43f;
		private readonly float _maxSpeed = 2.5f;
		private float _scale = 1f;

		private readonly Vector3 _initialPosition;
		private float _verticalVelocity;
		private bool _isGrounded;
		private bool _isVisible = true;

		private readonly BakedMeshSequence _meshSequence;
		private readonly Material _material;
		private readonly int _layer;
		private readonly string _currentAnimationClip = "Walk";
		private float _animationTime;
		private float _movementSpeed;
		private bool _isMoving;
		private Quaternion _rotation = Quaternion.identity;
		private float _speedBouns;
		private float2 _lastVelocity;
		private Vector3 _lastDirection;

		public Vector3 Position => _position;
		public int AgentId { get; private set; } = -1;

		public bool IsInitialized => AgentId != -1;

		public RVOAgent(Vector3 startPosition, BakedMeshSequence meshSequence, Material material, int layer = 0)
		{
			_position = startPosition;
			_initialPosition = startPosition;
			_meshSequence = meshSequence;
			_material = material;
			_layer = layer;
			_targetPosition = Vector3.zero;
			_animationTime = Random.value * meshSequence.GetClipDuration(_currentAnimationClip);
			_movementSpeed = 0f;

			_isMoving = false;
		}

		public void Initialize(Simulator simulator)
		{
			if(AgentId != -1)
			{
				return;
			}

			var position2D = new float2(_position.x, _position.z);
			AgentId = simulator.AddAgent(position2D);

			simulator.SetAgentRadius(AgentId, _radius * _scale);
			simulator.SetAgentMaxSpeed(AgentId, _maxSpeed / _scale);
			simulator.SetAgentTimeHorizonObst(AgentId, 0.2f * _scale);
			simulator.SetAgentNeighborDist(AgentId, 5f);
			simulator.SetAgentMaxNeighbors(AgentId, 4);
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
			_targetPosition = target;
		}

		public void UpdateAgent(Simulator simulator, float2 goal, float deltaTime)
		{
			if(AgentId == -1)
			{
				return;
			}

			var prevDistanceToTarget = PathFindManager.Instance.SampleFlowGradient(_position);
			_targetPosition = new Vector3(goal.x, 0, goal.y);
			var position2D = simulator.GetAgentPosition(AgentId);
			var distanceToTarget = math.lengthsq(position2D - goal);

			if(PathFindManager.Instance != null && _isGrounded)
			{
				SetPreferredVelocityFromFlowField(simulator);
			}
			else
			{
				SetPreferredVelocities(simulator, goal);
			}

			if(distanceToTarget < StoppingDistance * StoppingDistance)
			{
				simulator.SetAgentPosition(AgentId, new float2(_initialPosition.x, _initialPosition.z));
				_position = _initialPosition;
				_animationTime = Random.value * _meshSequence.GetClipDuration(_currentAnimationClip);
				return;
			}

			var prevPosition = _position;
			var wasGrounded = _isGrounded;
			_isGrounded = CheckGroundAndGetPosition(out var groundY);

			if(_isGrounded)
			{
				if(_position.y > groundY + 0.01f)
				{
					_verticalVelocity -= _gravityForce * deltaTime;
				}
				else
				{
					_verticalVelocity = 0f;
				}
			}
			else
			{
				_verticalVelocity -= _gravityForce * deltaTime;
			}

			var newY = prevPosition.y + _verticalVelocity * deltaTime;
			if(_isGrounded && newY <= groundY)
			{
				newY = groundY;
				_verticalVelocity = 0f;
			}

			var newPosition = new Vector3(position2D.x, newY, position2D.y);
			var movementDelta = newPosition - _position;
			_movementSpeed = movementDelta.magnitude / deltaTime;
			_isMoving = _movementSpeed > 0.1f;

			if(_isMoving)
			{
				var movementDirection = new Vector3(movementDelta.x, 0, movementDelta.z);
				if(movementDirection.sqrMagnitude > 0.001f)
				{
					movementDirection = movementDirection.normalized;
					_rotation = Quaternion.Lerp(_rotation, Quaternion.LookRotation(movementDirection, Vector3.up), deltaTime * 10f);
				}
			}

			_position = newPosition;
			var newDistanceToTarget = PathFindManager.Instance.SampleFlowGradient(_position);

			// if(newDistanceToTarget > prevDistanceToTarget)
			// {
			// 	_speedBouns += 0.12f;
			// 	simulator.SetAgentMaxSpeed(AgentId, _maxSpeed/_scale + _speedBouns);
			// }
			// else if(newDistanceToTarget < prevDistanceToTarget && _speedBouns > 0f)
			// {
			// 	_speedBouns -= 0.03f;
			// 	if(_speedBouns < 0)
			// 	{
			// 		_speedBouns = 0;
			// 	}
			// 	simulator.SetAgentMaxSpeed(AgentId, _maxSpeed/_scale + _speedBouns);
			// }
		}

		public Matrix4x4 GetTransformMatrix()
		{
			return Matrix4x4.TRS(_position + Vector3.up * (_scale - 1f), _rotation, Vector3.one * _scale + Vector3.up * _speedBouns);
		}

		public Mesh GetCurrentMesh()
		{
			if(_meshSequence == null)
			{
				return null;
			}

			if(_isMoving)
			{
				return _meshSequence.GetMeshAtTime(_animationTime, _currentAnimationClip);
			}
			return _meshSequence.GetMeshAtFrame(0);
		}

		public Material GetMaterial()
		{
			return _material;
		}

		public int GetLayer()
		{
			return _layer;
		}

		public bool IsVisible()
		{
			return _isVisible;
		}

		public void SetVisible(bool visible)
		{
			_isVisible = visible;
		}

		public void UpdateInstance(float deltaTime)
		{
			if(_isMoving && _meshSequence != null)
			{
				_animationTime += deltaTime / _scale;
				var clipDuration = _meshSequence.GetClipDuration(_currentAnimationClip);
				if(clipDuration > 0 && _animationTime >= clipDuration)
				{
					_animationTime = 0f;
				}
			}
		}

		public static float2 MoveTowards(float2 current, float2 target, float maxDistanceDelta)
		{
			var toTarget = target - current;
			var distance = math.length(toTarget);

			if(distance <= maxDistanceDelta)
			{
				return target;
			}

			var direction = toTarget / distance;
			var step = direction * maxDistanceDelta;

			return current + step;
		}

		public void UpdateRadialForce()
		{
			if(_scale > 1)
			{
				PathFindManager.Instance.SetDirectionalForce(Position + _lastDirection * _scale, _lastDirection, _radius * _scale * 2f, 1, 0.8f);
			}
		}

		private void SetRandomScale()
		{
			_scale = Random.value > 0.9f ? 1.5f : 1f;
		}

		private bool CheckGroundAndGetPosition(out float groundY)
		{
			var rayStart = _position + Vector3.up * (_capsuleHeight * 0.5f + _groundCheckOffset);
			var ray = new Ray(rayStart, Vector3.down);

			if(Physics.Raycast(ray, out var hit, _groundCheckDistance + _capsuleHeight * 0.5f + _groundCheckOffset, _groundLayerMask))
			{
				groundY = hit.point.y + _capsuleHeight * 0.5f;
				return true;
			}

			groundY = 0f;
			return false;
		}

		private void SetPreferredVelocityFromFlowField(Simulator simulator)
		{
			var flowDirection = PathFindManager.Instance.SampleFlowDirection(_position, _scale <= 1f);

			//var flowDirection = (_targetPosition-_position).normalized;
			var moveDirection = new Vector3(flowDirection.x, 0, flowDirection.y);
			var distanceToTarget = Vector3.Distance(_position, _targetPosition);

			float2 preferredVelocity;
			if(distanceToTarget > StoppingDistance && moveDirection.magnitude > 0.01f)
			{
				preferredVelocity = new float2(moveDirection.x, moveDirection.z) * _maxSpeed / _scale;
				preferredVelocity += (float2)Random.insideUnitCircle * 0.00001f;
			}
			else
			{
				preferredVelocity = float2.zero;
			}
			_lastVelocity = MoveTowards(_lastVelocity, math.normalizesafe(preferredVelocity), Time.deltaTime * 2f);
			_lastDirection = new Vector3(_lastVelocity.x, 0, _lastVelocity.y);
			var flowVector = new Vector3(_lastVelocity.x, 0, _lastVelocity.y);
			var headPos = _position + Vector3.up * _scale;
			Debug.DrawLine(headPos, headPos + flowVector);
			simulator.SetAgentPrefVelocity(AgentId, _lastVelocity);
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
