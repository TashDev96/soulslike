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

		private Vector3 _position;
		private Vector3 _targetPosition;
		private readonly float _gravityForce = 9.81f;
		private readonly float _groundCheckDistance = 2f;
		private readonly float _groundCheckOffset = 0.1f;
		private readonly LayerMask _groundLayerMask = -1;
		private readonly float _capsuleHeight = 2f;
		private readonly float _radius = 0.43f;
		private readonly float _maxSpeed = 2.5f;
		private float _scale;

		private AiNavigationModule _navigationModule;
		private float _pathRecalculationTimer;
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

		public Vector3 Position => _position;
		public int AgentId { get; private set; } = -1;

		public bool IsInitialized => AgentId != -1;

		public RVOAgent(Vector3 startPosition, BakedMeshSequence meshSequence, Material material, int layer = 0)
		{
			_position = startPosition;
			_initialPosition = startPosition;
			this._meshSequence = meshSequence;
			this._material = material;
			this._layer = layer;
			_scale = _scale;
			_targetPosition = Vector3.zero;
			_animationTime = Random.value * meshSequence.GetClipDuration(_currentAnimationClip);
			_movementSpeed = 0f;
			SetRandomScale();
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
			simulator.SetAgentMaxSpeed(AgentId, _maxSpeed/_scale);
			simulator.SetAgentTimeHorizonObst(AgentId, 4f*_scale);

			_navigationModule = new AiNavigationModule();
			_pathRecalculationTimer = Random.value;
			_navigationModule.BuildPath(_position, _targetPosition);
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
			if(_navigationModule != null)
			{
				_navigationModule.BuildPath(_position, target);
			}
		}

		public void UpdateAgent(Simulator simulator, float2 goal, float deltaTime)
		{
			if(AgentId == -1)
			{
				return;
			}

			_pathRecalculationTimer += deltaTime;
			SetPreferredVelocities(simulator, goal);

			_targetPosition = new Vector3(goal.x, 0, goal.y);
			var position2D = simulator.GetAgentPosition(AgentId);
			var distanceToTarget = math.lengthsq(position2D - goal);
			
			var headPos = _position + Vector3.up * _scale;

			var recalculatePathDelay = Mathf.Lerp(0, 5, distanceToTarget / 20f);

			if(_pathRecalculationTimer >= PathRecalculationInterval + recalculatePathDelay)
			{
				_pathRecalculationTimer = 0f;
				if(_targetPosition != Vector3.zero && _navigationModule != null)
				{
					//navigationModule.BuildPath(position, targetPosition);
				}
			}

			if(_navigationModule != null && _navigationModule.HasPath && _isGrounded)
			{
				SetPreferredVelocityFromNavMesh(simulator, out var navmeshVector);
				Debug.DrawLine(headPos, headPos+navmeshVector);
				
			}

			if(distanceToTarget < StoppingDistance * StoppingDistance)
			{
				//restart
				simulator.SetAgentPosition(AgentId, new float2(_initialPosition.x, _initialPosition.z));
				_position = _initialPosition;
				_animationTime = Random.value * _meshSequence.GetClipDuration(_currentAnimationClip);

				if(_navigationModule != null)
				{
					_navigationModule.BuildPath(_position, _targetPosition);
				}
				return;
			}

			var prevPosition = _position;
			var wasGrounded = _isGrounded;
			_isGrounded = CheckGroundAndGetPosition(out var groundY);

			if(_isGrounded)
			{
				if(!wasGrounded && _navigationModule != null)
				{
					_navigationModule.BuildPath(_position, _targetPosition);
				}
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
				var movementDirection = new Vector3(movementDelta.x, 0, movementDelta.z).normalized;
				_rotation = Quaternion.Lerp(_rotation, Quaternion.LookRotation(movementDirection, Vector3.up), deltaTime * 10f);
			}

			_position = newPosition;

			
		}

		public Matrix4x4 GetTransformMatrix()
		{
			return Matrix4x4.TRS(_position+Vector3.up*(_scale-1f), _rotation, Vector3.one * _scale);
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

		private void SetRandomScale()
		{
			_scale = Random.value > 0.9f ? 2f : 1f;
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

		private void SetPreferredVelocityFromNavMesh(Simulator simulator, out Vector3 navmeshVector)
		{
			var moveDirection = _navigationModule.CalculateMoveDirection(_position, 3f, 1.5f);
			var distanceToTarget = Vector3.Distance(_position, _navigationModule.TargetPosition);
			navmeshVector = moveDirection;
			float2 preferredVelocity;
			if(distanceToTarget > StoppingDistance)
			{
				preferredVelocity = new float2(moveDirection.x, moveDirection.z)*_maxSpeed;
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
