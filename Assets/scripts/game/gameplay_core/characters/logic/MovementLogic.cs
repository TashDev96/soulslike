using System;
using System.Linq;
using System.Text;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.drawers;
using game.gameplay_core.characters.config;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.input;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	[Serializable]
	public class MovementLogic
	{
		public struct Context
		{
			public Transform CharacterTransform;
			public CapsuleCharacterCollider CharacterCollider;
			public LocomotionConfig LocomotionConfig;
			public IsDead IsDead { get; set; }
			public ReactiveProperty<RotationSpeedData> RotationSpeed { get; set; }
			public ReactiveProperty<bool> IsFalling { get; set; }
		}

		[SerializeField]
		private float _inAirDamping;
		[SerializeField]
		private float _slidingAcceleration;
		[SerializeField]
		private float _slidingDamping;
		[SerializeField]
		private float _slidingStopDamping;

		[Space]
		[SerializeField]
		private bool _drawDebug;

		private Context _context;

		private Vector3 _slidingVelocity;

		private LayerMask _groundLayer;
		private Vector3 _groundNormal;
		private bool _hasStableGround;

		private Vector3 _prevPos;
		private bool _isGroundedCache;
		private bool _prevIsGrounded;
		private Vector3 _lastUpdateVelocity;

		private Vector3 _fallVelocity;
		private CollisionFlags _debugFlags;

		private CapsuleCharacterCollider CharacterCollider => _context.CharacterCollider;

		private Vector3 CurrentPosition => _context.CharacterTransform.position;

		public void SetContext(Context context)
		{
			_context = context;
			_context.IsDead.OnChanged += HandleDeath;
			_groundLayer = LayerMask.GetMask("Default");
			_prevPos = CurrentPosition;
		}

		public void Update(float deltaTime)
		{
			_prevIsGrounded = _isGroundedCache;
			_isGroundedCache = false;

			if(_context.IsDead.Value)
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

			_lastUpdateVelocity = (CurrentPosition - _prevPos) / deltaTime;
			_prevPos = CurrentPosition;

		}

		public void Walk(Vector3 vector, float deltaTime)
		{
			var projectedMovement = Vector3.ProjectOnPlane(vector, _groundNormal);

			if(_isGroundedCache)
			{
				if(_hasStableGround)
				{
					_slidingVelocity = Vector3.MoveTowards(_slidingVelocity, Vector3.zero, deltaTime * 10f);

					if(projectedMovement.magnitude > 0.01f)
					{
						//TryStepUpStairs(projectedMovement);
					}
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
			var degreesPerSecond = 180f / _context.LocomotionConfig.HalfTurnDurationSeconds;
			RotateCharacter(toDirection, degreesPerSecond, deltaTime);
		}

		public void RotateCharacter(Vector3 toDirection, float speed, float deltaTime)
		{
			toDirection.y = 0;
			var angleDifference = Vector3.SignedAngle(_context.CharacterTransform.forward, toDirection, Vector3.up);
			var clampedAngle = Mathf.Clamp(angleDifference, -speed * deltaTime, speed * deltaTime);
			var rotationStep = Quaternion.AngleAxis(clampedAngle, Vector3.up);

			_context.CharacterTransform.rotation *= rotationStep;
		}

		public bool CheckGroundBelow(float maxDistance, out float distanceToGround)
		{
			var charController = CharacterCollider;
			var radius = charController.radius;

			var offset = radius + charController.skinWidth;

			var origin = _context.CharacterTransform.position + Vector3.up * offset;

			var hitResults = new RaycastHit[5];
			var hitCount = Physics.SphereCastNonAlloc(origin, radius, Vector3.down, hitResults, maxDistance + offset, _groundLayer);

			if(hitCount > 0)
			{
				distanceToGround = maxDistance + radius;

				for(var i = 0; i < hitCount; i++)
				{
					var hitDistance = hitResults[i].distance;
					if(hitDistance < distanceToGround)
					{
						distanceToGround = hitDistance;
					}
				}

				if(_drawDebug)
				{
					//DebugDrawUtils.DrawHandlesSphere(origin, radius / 2, new Color(1, 0, 1, 0.3f));
					//DebugDrawUtils.DrawHandlesSphere(origin + Vector3.down * distanceToGround, radius, Color.green, 0.3f);

					//DebugDrawUtils.DrawText(distanceToGround.RoundFormat(), origin + Vector3.down * (distanceToGround / 2), 10f);
				}

				return true;
			}

			if(_drawDebug)
			{
				//DebugDrawUtils.DrawHandlesSphere(origin, radius / 2, new Color(1, 0, 1, 0.3f));
				//DebugDrawUtils.DrawHandlesSphere(origin + Vector3.down * maxDistance, radius, Color.red, 0.3f);
			}

			distanceToGround = 0f;
			return false;
		}

		public void GetDebugString(StringBuilder sb)
		{
			sb.AppendLine($"grounded {_isGroundedCache}, stable: {_hasStableGround}, gravity disabled: {_context.CharacterCollider.IsFakeGrounded}");
			sb.AppendLine($"fall velocity {_fallVelocity}");
			sb.AppendLine($"Collision Flags: {string.Join(", ", Enum.GetValues(typeof(CollisionFlags)).Cast<CollisionFlags>().Distinct().Where(f => (_debugFlags & f) == f && f != CollisionFlags.None))}");
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
			
			

			if(_isGroundedCache)
			{
				_context.IsFalling.Value = false;
				_fallVelocity = Vector3.zero;
			}
			else
			{
				if(_prevIsGrounded)
				{
					_fallVelocity = _lastUpdateVelocity;
					if(_fallVelocity.y > 0)
					{
						//avoid trampline effect
						_fallVelocity.y = 0;
					}
				}
				
				if(_context.IsFalling.Value)
				{
					_fallVelocity += Physics.gravity * deltaTime;
					_fallVelocity = Vector3.Lerp(_fallVelocity, Vector3.zero, _inAirDamping * deltaTime);

					MoveAndStoreFrameData(_fallVelocity * deltaTime);
				}

				if(!CharacterCollider.IsGrounded)
				{
					if(!_context.IsFalling.Value && CheckGroundBelow(CharacterCollider.stepOffset, out var distanceToGround))
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
			if(_hasStableGround || CharacterCollider.IsOnStableSlope)
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
			DebugDrawUtils.DrawText(isFalling ? "start fall" : "end fall", _context.CharacterTransform.position, 2f);
		}
	}
}
