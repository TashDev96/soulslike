using System;
using System.Linq;
using System.Text;
using dream_lib.src.reactive;
using dream_lib.src.utils.data_types;
using dream_lib.src.utils.drawers;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	[Serializable]
	public class MovementLogic
	{
		private const float AirDamping = 0.33f;
		[SerializeField]
		private float _slidingAcceleration;
		[SerializeField]
		private float _slidingDamping;
		[SerializeField]
		private float _slidingStopDamping;

		[Space]
		[SerializeField]
		private bool _drawDebug;

		private CharacterContext _context;

		private Vector3 _slidingVelocity;

		private Vector3 _groundNormal;

		private Vector3 _prevPos;
		private bool _isGroundedCache;
		private bool _prevIsGrounded;

		private Vector3 _fallVelocity;
		private CollisionFlags _debugFlags;
		private Vector3 _acceleratedMovement;
		private bool _hadAcceleratedMovement;
		private Vector3 _virtualForward;
		private bool _rotationMovementLocked;
		private Transform _transform;
		public Vector3 LastUpdateVelocity { get; private set; }

		private CapsuleCharacterCollider CharacterCollider => _context.CharacterCollider;

		private Vector3 CurrentPosition => _context.Transform.Position;

		public void SetContext(CharacterContext context, Transform transform)
		{
			_context = context;
			_transform = transform;
			_context.IsDead.OnChanged += HandleDeath;
			_prevPos = CurrentPosition;
			_virtualForward = _context.Transform.Forward;
		}

		public void Update(float deltaTime)
		{
			_prevIsGrounded = _isGroundedCache;
			_isGroundedCache = false;

			if(_context.IsDead.Value)
			{
				return;
			}

			if(_rotationMovementLocked)
			{
				return;
			}

			_debugFlags = _context.CharacterCollider.Flags;

			_context.CharacterCollider.CustomUpdate(deltaTime);

			UpdateFalling(deltaTime);

			if(_isGroundedCache)
			{
				UpdateSliding(deltaTime);
			}

			if(_hadAcceleratedMovement)
			{
				_hadAcceleratedMovement = false;
			}
			else
			{
				_acceleratedMovement = Vector3.MoveTowards(_acceleratedMovement, Vector3.zero, deltaTime * _context.Config.Locomotion.WalkDeceleration);
			}

			LastUpdateVelocity = (CurrentPosition - _prevPos) / deltaTime;
			_prevPos = CurrentPosition;
		}

		public void ApplyLocomotion(Vector3 vector, float deltaTime)
		{
			if(_rotationMovementLocked)
			{
				return;
			}
			var projectedMovement = Vector3.ProjectOnPlane(vector, _groundNormal);

			if(_isGroundedCache)
			{
				if(CharacterCollider.HasStableGround)
				{
					_slidingVelocity = Vector3.MoveTowards(_slidingVelocity, Vector3.zero, deltaTime * 10f);
				}
				else
				{
					projectedMovement -= Vector3.Project(projectedMovement, _slidingVelocity.normalized);
				}
			}
			MoveAndStoreFrameData(projectedMovement);
		}

		public void RotateCharacter(Vector3 toDirection, float deltaTime)
		{
			RotateCharacter(toDirection, _context.Config.Locomotion.HalfTurnDurationSeconds, deltaTime);
		}

		public void RotateCharacter(Vector3 toDirection, float halfTurnDurationSeconds, float deltaTime)
		{
			if(_rotationMovementLocked)
			{
				return;
			}
			var degreesPerSecond = 180f / halfTurnDurationSeconds;

			toDirection.y = 0;
			var angleDifference = Vector3.SignedAngle(_context.Transform.Forward, toDirection, Vector3.up);
			var clampedAngle = Mathf.Clamp(angleDifference, -degreesPerSecond * deltaTime, degreesPerSecond * deltaTime);
			var rotationStep = Quaternion.AngleAxis(clampedAngle, Vector3.up);

			_transform.rotation *= rotationStep;
			if(!_context.LockOnLogic.LockOnTarget.HasValue)
			{
				_virtualForward = _context.Transform.Forward;
			}
		}

		public void ApplyInputMovement(Vector3 inputDirection, float speed, float deltaTime)
		{
			if(_rotationMovementLocked)
			{
				return;
			}
			var hasLockOnTarget = _context.LockOnLogic.LockOnTarget.HasValue;

			if(!hasLockOnTarget)
			{
				RotateCharacter(inputDirection, deltaTime);
			}
			else
			{
				if(inputDirection.sqrMagnitude > 0.001f)
				{
					var targetForward = inputDirection.normalized;
					targetForward.y = 0;
					targetForward = targetForward.normalized;
					var degreesPerSecond = 180f / _context.Config.Locomotion.HalfTurnDurationSeconds;
					var angleDifference = Vector3.SignedAngle(_virtualForward, targetForward, Vector3.up);
					var clampedAngle = Mathf.Clamp(angleDifference, -degreesPerSecond * deltaTime, degreesPerSecond * deltaTime);
					var rotationStep = Quaternion.AngleAxis(clampedAngle, Vector3.up);
					_virtualForward = (rotationStep * _virtualForward).normalized;
				}
			}

			var directionMultiplier = Mathf.Clamp01(Vector3.Dot(_virtualForward, inputDirection));
			var velocity = inputDirection * (directionMultiplier * speed);
			ApplyLocomotion(velocity * deltaTime, deltaTime);
		}

		public void GetDebugString(StringBuilder sb)
		{
			sb.AppendLine($"grounded {_isGroundedCache}/{CharacterCollider.IsGrounded}, stable: {CharacterCollider.HasStableGround}, gravity disabled: {_context.CharacterCollider.IsFakeGrounded}");
			sb.AppendLine($"falling: {_context.IsFalling.Value}  fall velocity {_fallVelocity}");
			sb.AppendLine($"Collision Flags: {string.Join(", ", Enum.GetValues(typeof(CollisionFlags)).Cast<CollisionFlags>().Distinct().Where(f => (_debugFlags & f) == f && f != CollisionFlags.None))}");
		}

		public void SetRotationAndMovementLocked(bool value)
		{
			_rotationMovementLocked = value;
		}

		public void Teleport(TransformCache respawnTransform)
		{
			_context.SelfLink.transform.position = respawnTransform.Position;
			_context.SelfLink.transform.eulerAngles = respawnTransform.EulerAngles;
		}

		private void MoveWithAcceleration(Vector3 vector, float deltaTime)
		{
			_hadAcceleratedMovement = true;
			var projectedMovement = Vector3.ProjectOnPlane(vector, _groundNormal);

			_acceleratedMovement = Vector3.MoveTowards(_acceleratedMovement, projectedMovement, deltaTime * _context.Config.Locomotion.WalkAcceleration);
			var resultMovement = _acceleratedMovement;

			if(_isGroundedCache)
			{
				if(CharacterCollider.HasStableGround)
				{
					_slidingVelocity = Vector3.MoveTowards(_slidingVelocity, Vector3.zero, deltaTime * 10f);
				}
				else
				{
					resultMovement -= Vector3.Project(resultMovement, _slidingVelocity.normalized);
				}
			}
			MoveAndStoreFrameData(resultMovement);
		}

		private void UpdateFalling(float deltaTime)
		{
			if(_context.CharacterCollider.IsFakeGrounded)
			{
				_isGroundedCache = true;
			}
			else
			{
				MoveAndStoreFrameData(Vector3.down * 0.0001f, true);
			}
			_debugFlags = _context.CharacterCollider.Flags;

			if(_isGroundedCache)
			{
				_context.IsFalling.Value = false;
				_fallVelocity = Vector3.zero;
			}
			else
			{
				if(_prevIsGrounded)
				{
					_fallVelocity = LastUpdateVelocity;
					if(_fallVelocity.y > 0)
					{
						//avoid trampline effect
						_fallVelocity.y = 0;
					}
				}

				if(_context.IsFalling.Value)
				{
					if(AirDamping > 0f && _fallVelocity.sqrMagnitude > 0.0001f)
					{
						var velocityMagnitude = _fallVelocity.magnitude;
						var dampingForce = -_fallVelocity.normalized * velocityMagnitude * AirDamping;
						_fallVelocity += dampingForce * deltaTime;
					}

					_fallVelocity += Physics.gravity * deltaTime;

					MoveAndStoreFrameData(_fallVelocity * deltaTime);
				}

				if(!CharacterCollider.IsGrounded)
				{
					if(!_context.IsFalling.Value && CharacterCollider.SampleGroundBelow(CharacterCollider.StepOffset, out var distanceToGround))
					{
						MoveAndStoreFrameData(Vector3.down * (distanceToGround + 0.0001f), true);
					}
					else
					{
						_context.IsFalling.Value = true;
					}
				}
			}
		}

		private void UpdateSliding(float deltaTime)
		{
			if(CharacterCollider.HasStableGround || CharacterCollider.IsOnStableSlope)
			{
				_slidingVelocity.y = 0;
				_slidingVelocity = Vector3.Lerp(_slidingVelocity, Vector3.zero, deltaTime * _slidingStopDamping);
				_slidingVelocity = Vector3.MoveTowards(_slidingVelocity, Vector3.zero, deltaTime);
				MoveAndStoreFrameData(_slidingVelocity * deltaTime);
				if(_slidingVelocity.sqrMagnitude < 0.001f)
				{
					_slidingVelocity = Vector3.zero;
				}
			}
			else
			{
				var slideDirection = Vector3.ProjectOnPlane(Vector3.down, _groundNormal).normalized;
				_slidingVelocity = Vector3.Lerp(_slidingVelocity, Vector3.zero, deltaTime * _slidingDamping);
				_slidingVelocity += slideDirection * deltaTime * _slidingAcceleration;
				MoveAndStoreFrameData(_slidingVelocity * deltaTime);
			}
		}

		private void MoveAndStoreFrameData(Vector3 vector, bool disableIterations = false)
		{
			CharacterCollider.Move(vector, disableIterations);

			//this is required because UnityCharacterController.isGrounded is changed every time Move() called
			_isGroundedCache |= CharacterCollider.IsGrounded;
		}

		private void HandleDeath(bool isDead)
		{
			CharacterCollider.enabled = !isDead;
		}

		private void HandleFallingChanged(bool isFalling)
		{
			DebugDrawUtils.DrawText(isFalling ? "start fall" : "end fall", _context.Transform.Position, 2f);
		}
	}
}
