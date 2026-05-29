using System;
using System.Collections.Generic;
using System.Text;
using dream_lib.src.utils.data_types;
using dream_lib.src.utils.drawers;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	[Serializable]
	public class MovementLogic
	{
		private const float AirDamping = 0.33f;

		private const float SlidingAcceleration = 10;
		private const float SlidingDamping = 0.7f;
		private const float SlidingStopDamping = 2;

		private CharacterContext _context;

		private Vector3 _prevPos;

		private IsGroundedData _isGrounded;
		private Vector3 _groundNormal;

		private bool _rotationAndMovementLocked;
		private readonly HashSet<string> _rotationLockReasons = new();

		private Vector3 _slidingVelocity;
		private Vector3 _fallVelocity;
		private CollisionFlags _debugFlags;
		private Vector3 _acceleratedMovement;
		private bool _hadAcceleratedMovement;
		private Vector3 _virtualForward;
		private bool _rotationMovementLocked;
		private Vector3 _compensateTeleportAmount;
		public bool LockedInAnimationSlot { get; set; }

		public Vector3 FallVelocity => _fallVelocity;

		public bool RotationIsControlledByCamera { get; set; }
		public Vector3 LastUpdateVelocity { get; private set; }

		private CapsuleCharacterCollider CharacterCollider => _context.CharacterCollider;

		private Vector3 CurrentPosition => _context.Transform.Position;

		public void SetContext(CharacterContext context)
		{
			_context = context;
			_context.IsDead.OnChanged += HandleDeath;
			_context.Events.ApplyDamage.OnExecute += HandleDamage;
			_prevPos = CurrentPosition;
			_virtualForward = _context.Transform.Forward;
		}

		public void Update(float deltaTime)
		{
			_isGrounded.Cached = false;
			if(_isGrounded.ContinuousGroundingTriggered && Time.frameCount > _isGrounded.GroundingConfirmFrame)
			{
				_isGrounded.ContinuousGroundingTriggered = false;
			}

			_debugFlags = _context.CharacterCollider.Flags;

			_context.CharacterCollider.CustomUpdate(deltaTime);

			if(LockedInAnimationSlot)
			{
				CalculateLastUpdateVelocity(deltaTime);
				return;
			}

			UpdateFalling(deltaTime);
			UpdateSliding(deltaTime);

			if(!_rotationAndMovementLocked)
			{
				if(_hadAcceleratedMovement)
				{
					_hadAcceleratedMovement = false;
				}
				else
				{
					_acceleratedMovement = Vector3.MoveTowards(_acceleratedMovement, Vector3.zero, deltaTime * _context.CharacterStats.Locomotion.WalkDeceleration);
				}
			}

			CalculateLastUpdateVelocity(deltaTime);
			_isGrounded.Previous = _isGrounded.Continuous;

			if(!_isGrounded.Cached)
			{
				_isGrounded.Continuous = false;
				_isGrounded.ContinuousGroundingTriggered = false;
			}
		}

		public void ApplyLocomotion(Vector3 vector, float deltaTime)
		{
			if(_rotationAndMovementLocked || LockedInAnimationSlot)
			{
				return;
			}
			var projectedMovement = Vector3.ProjectOnPlane(vector, _groundNormal);

			if(_isGrounded.Continuous)
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
			RotateCharacter(toDirection, _context.CharacterStats.Locomotion.HalfTurnDurationSeconds, deltaTime);
		}

		public void RotateCharacter(Vector3 toDirection, float halfTurnDurationSeconds, float deltaTime)
		{
			if(_rotationAndMovementLocked || _rotationLockReasons.Count > 0 || LockedInAnimationSlot)
			{
				return;
			}

			if(RotationIsControlledByCamera)
			{
				_virtualForward = _context.InputData.DirectionWorld;
				return;
			}

			var degreesPerSecond = 180f / halfTurnDurationSeconds;

			toDirection.y = 0;
			var angleDifference = Vector3.SignedAngle(_context.Transform.Forward, toDirection, Vector3.up);
			var clampedAngle = Mathf.Clamp(angleDifference, -degreesPerSecond * deltaTime, degreesPerSecond * deltaTime);
			var rotationStep = Quaternion.AngleAxis(clampedAngle, Vector3.up);

			_context.Transform.Rotation *= rotationStep;
			if(!_context.Logic.LockOnLogic.LockOnTarget.HasValue)
			{
				_virtualForward = _context.Transform.Forward;
			}
		}

		public void ApplyInputMovement(Vector3 inputDirection, float speed, float deltaTime)
		{
			if(_rotationAndMovementLocked || LockedInAnimationSlot)
			{
				return;
			}
			var hasLockOnTarget = _context.Logic.LockOnLogic.LockOnTarget.HasValue;

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
					var degreesPerSecond = 180f / _context.CharacterStats.Locomotion.HalfTurnDurationSeconds;
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
			sb.Append("Rotation Locked: ").Append(_rotationLockReasons.Count).AppendLine();
			var target = _context.Logic.LockOnLogic.LockOnTarget.Value;

			sb.Append("Target Locked: ").Append(target != null ? target.name : "None").AppendLine();

			sb.Append($"is grounded {_isGrounded.Continuous}").AppendLine();
			sb.Append($"is falling {_context.IsFalling.Value}").AppendLine();
		}

		public void SetRotationAndMovementLocked(bool value)
		{
			_rotationAndMovementLocked = value;
		}

		public void SetRotationLockedBy(string source, bool locked)
		{
			if(locked)
			{
				_rotationLockReasons.Add(source);
			}
			else
			{
				_rotationLockReasons.Remove(source);
			}
		}

		public void Teleport(TransformCache respawnTransform)
		{
			var prevPos = _context.Transform.Position;
			_context.Transform.SetPosition(respawnTransform.Position);
			_context.Transform.SetRotation(respawnTransform.EulerAngles);
			_compensateTeleportAmount += _context.Transform.Position - prevPos;
		}

		public static Vector3 GetAirDampingForceFalling(Vector3 velocity)
		{
			var velocityMagnitude = velocity.magnitude;
			return -velocity.normalized * (velocityMagnitude * AirDamping);
		}

		public void ResetVelocity()
		{
			_slidingVelocity = Vector3.zero;
			_fallVelocity = Vector3.zero;
		}

		public void AddFallImpulse(Vector3 impulse)
		{
			_fallVelocity += impulse;
		}

		public void SetFallVelocity(Vector3 value)
		{
			_fallVelocity = value;
		}

		private void CalculateLastUpdateVelocity(float deltaTime)
		{
			LastUpdateVelocity = (CurrentPosition - _prevPos) / deltaTime - _compensateTeleportAmount / deltaTime;
			_compensateTeleportAmount = Vector3.zero;
			_prevPos = CurrentPosition;
		}

		private void HandleDamage(DamageInfo info)
		{
			_slidingVelocity += info.Direction * info.KnockbackImpulse / _context.RigidBody.Mass;
		}

		private void UpdateFlyingMode(float deltaTime)
		{
		}

		private void MoveWithAcceleration(Vector3 vector, float deltaTime)
		{
			_hadAcceleratedMovement = true;
			var projectedMovement = Vector3.ProjectOnPlane(vector, _groundNormal);

			_acceleratedMovement = Vector3.MoveTowards(_acceleratedMovement, projectedMovement, deltaTime * _context.CharacterStats.Locomotion.WalkAcceleration);
			var resultMovement = _acceleratedMovement;

			if(_isGrounded.Continuous)
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
			if(_context.DebugVars.DisableFall)
			{
				return;
			}

			if(_context.CharacterCollider.IsSteppingUp)
			{
				_isGrounded.Continuous = true;
				_isGrounded.Continuous = true;
			}
			else
			{
				MoveAndStoreFrameData(Vector3.down * 0.0001f, true);
			}
			_debugFlags = _context.CharacterCollider.Flags;

			if(_isGrounded.Continuous)
			{
				if(_context.IsFalling.Value)
				{
					_context.IsFalling.Value = false;
					_fallVelocity = Vector3.zero;
				}
			}
			else
			{
				if(_isGrounded.Previous)
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
						_fallVelocity += GetAirDampingForceFalling(_fallVelocity) * deltaTime;
					}

					_fallVelocity += Physics.gravity * deltaTime;
					_context.DebugVars.IsFallCall = true;
					MoveAndStoreFrameData(_fallVelocity * deltaTime);
					_context.DebugVars.IsFallCall = false;
				}
				else
				{
					var hasGroundPreventFalling = CharacterCollider.SampleGroundBelow(CharacterCollider.StepOffset, out var distanceToGround, 0.1f);
					if(!_context.IsFalling.Value && hasGroundPreventFalling)
					{
						MoveAndStoreFrameData(Vector3.down * (distanceToGround + 0.0001f), true, true);
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
			if(_isGrounded.Continuous)
			{
				if(CharacterCollider.HasStableGround || CharacterCollider.IsOnStableSlope)
				{
					_slidingVelocity.y = 0;
					_slidingVelocity = Vector3.Lerp(_slidingVelocity, Vector3.zero, deltaTime * SlidingStopDamping);
					_slidingVelocity = Vector3.MoveTowards(_slidingVelocity, Vector3.zero, deltaTime);
					if(_slidingVelocity.sqrMagnitude < 0.001f)
					{
						_slidingVelocity = Vector3.zero;
					}
				}
				else
				{
					var slideDirection = Vector3.ProjectOnPlane(Vector3.down, _groundNormal).normalized;
					_slidingVelocity = Vector3.Lerp(_slidingVelocity, Vector3.zero, deltaTime * SlidingDamping);
					_slidingVelocity += slideDirection * deltaTime * SlidingAcceleration;
				}
			}

			if(!_context.IsFalling.Value)
			{
				MoveAndStoreFrameData(_slidingVelocity * deltaTime);
			}
		}

		private void MoveAndStoreFrameData(Vector3 vector, bool disableIterations = false, bool dontCountAsSpeed = false)
		{
			var prevPos = _context.Transform.Position;
			CharacterCollider.Move(vector, disableIterations);

			var posDelta = _context.Transform.Position - prevPos;

			if(dontCountAsSpeed)
			{
				_compensateTeleportAmount += posDelta;
			}

			//this is required because UnityCharacterController.isGrounded is invalidated every time Move() called
			_isGrounded.Cached |= CharacterCollider.IsGrounded;
			if(_isGrounded is { Cached: true, Continuous: false })
			{
				if(!_isGrounded.ContinuousGroundingTriggered)
				{
					_isGrounded.ContinuousGroundingTriggered = true;
					_isGrounded.GroundingConfirmFrame = Time.frameCount + 2;
				}
				else
				{
					if(Time.frameCount >= _isGrounded.GroundingConfirmFrame)
					{
						_isGrounded.Continuous = true;
						_isGrounded.ContinuousGroundingTriggered = false;
					}
				}
			}
		}

		private void HandleDeath(bool isDead)
		{
			CharacterCollider.enabled = !isDead;
		}

		private void HandleFallingChanged(bool isFalling)
		{
			DebugDrawUtils.DrawText(isFalling ? "start fall" : "end fall", _context.Transform.Position, 2f);
		}

		private struct IsGroundedData
		{
			public bool Cached; //invalidates each update
			public bool Continuous;
			public bool Previous;
			public bool ContinuousGroundingTriggered;
			public int GroundingConfirmFrame;
		}
	}
}
