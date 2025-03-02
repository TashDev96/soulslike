using System;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.drawers;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	[Serializable]
	public class MovementLogic
	{
		public struct Context
		{
			public Transform CharacterTransform;
			public CharacterController UnityCharacterController;
			public IsDead IsDead { get; set; }
			public ReactiveProperty<RotationSpeedData> RotationSpeed { get; set; }

			// Uses the shared IsFalling property
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

		private readonly RaycastHit[] _groundCastResults = new RaycastHit[20];
		private LayerMask _groundLayer;
		private Vector3 _groundNormal;
		private bool _hasStableGround;

		private Vector3 _prevPos;
		private bool _isGroundedCache;
		private bool _prevIsGrounded;

		private CharacterController UnityCharacterController => _context.UnityCharacterController;

		private Vector3 CurrentPosition => _context.CharacterTransform.position;

		public Vector3 FallVelocity { get; private set; }

		public void SetContext(Context context)
		{
			_context = context;
			_context.IsDead.OnChanged += HandleDeath;
			_groundLayer = LayerMask.GetMask("Default");
			_prevPos = CurrentPosition;
		}

		public void Update(float deltaTime)
		{
			if(_context.IsDead.Value)
			{
				return;
			}

			UpdateFalling(deltaTime);
			GetGroundNormal();

			if(UnityCharacterController.isGrounded)
			{
				UpdateSliding(deltaTime);
			}

			if(_drawDebug && Time.frameCount % 10 == 0)
			{
				DebugDrawUtils.DrawText(_slidingVelocity.magnitude.RoundFormat(), CurrentPosition + Vector3.up, 10f);
			}
			_prevPos = CurrentPosition;
			_prevIsGrounded = _isGroundedCache;
			_isGroundedCache = false;
		}

		public void Walk(Vector3 vector, float deltaTime)
		{
			var projectedMovement = Vector3.ProjectOnPlane(vector, _groundNormal);

			if(UnityCharacterController.isGrounded)
			{
				if(_hasStableGround)
				{
					_slidingVelocity = Vector3.MoveTowards(_slidingVelocity, Vector3.zero, deltaTime * 10f);
				}
				else
				{
					projectedMovement -= Vector3.Project(projectedMovement, _slidingVelocity.normalized);
				}
			}
			MoveAndStoreIsGrounded(projectedMovement);
		}

		public void RotateCharacter(Vector3 toDirection, float deltaTime)
		{
			RotateCharacter(toDirection, _context.RotationSpeed.Value.DegreesPerSecond, deltaTime);
		}

		public void RotateCharacter(Vector3 toDirection, float speed, float deltaTime)
		{
			toDirection.y = 0;
			var angleDifference = Vector3.SignedAngle(_context.CharacterTransform.forward, toDirection, Vector3.up);
			var clampedAngle = Mathf.Clamp(angleDifference, -speed * deltaTime, speed * deltaTime);
			var rotationStep = Quaternion.AngleAxis(clampedAngle, Vector3.up);

			_context.CharacterTransform.rotation *= rotationStep;
		}

		private void GetGroundNormal()
		{
			if(!UnityCharacterController.isGrounded)
			{
				_groundNormal = Vector3.up;
			}

			var capsule = UnityCharacterController;

			var origin = _context.CharacterTransform.position + capsule.center;
			var radius = capsule.radius + UnityCharacterController.skinWidth;
			var maxDistance = capsule.height / 2 + 0.0001f - radius + UnityCharacterController.skinWidth;

			var count = Physics.SphereCastNonAlloc(origin, radius, Vector3.down, _groundCastResults, maxDistance, _groundLayer);

			if(_drawDebug)
			{
				DebugDrawUtils.DrawHandlesSphere(origin, radius, new Color(1, 0, 0, 0.3f));
				DebugDrawUtils.DrawHandlesSphere(origin + Vector3.down * maxDistance, radius, new Color(0, 1, 0, 0.3f));
			}

			if(count == 0)
			{
				_groundNormal = Vector3.up;
				return;
			}

			_hasStableGround = false;
			var normalsSum = Vector3.zero;

			for(var i = 0; i < count; i++)
			{
				var result = _groundCastResults[i];
				var angle = Vector3.Angle(Vector3.up, result.normal);
				var isSliding = angle > UnityCharacterController.slopeLimit;
				if(!isSliding)
				{
					_hasStableGround = true;
					_groundNormal = result.normal;
				}

				normalsSum += result.normal;

				if(_drawDebug)
				{
					Debug.DrawRay(result.point, result.normal * 2, isSliding ? Color.red : Color.green);
				}
			}

			if(!_hasStableGround)
			{
				_groundNormal = normalsSum / count;
			}
		}

		private void UpdateFalling(float deltaTime)
		{
			MoveAndStoreIsGrounded(Vector3.down * 0.0001f);
			
			if(_isGroundedCache)
			{
				_context.IsFalling.Value = false;
				FallVelocity = Vector3.zero;
			}
			else
			{
				if(_prevIsGrounded)
				{
					FallVelocity = _slidingVelocity;
				}
				_context.IsFalling.Value = true;
				FallVelocity = Vector3.Lerp(FallVelocity, Vector3.zero, _inAirDamping * deltaTime);
				FallVelocity += Physics.gravity * deltaTime;
				MoveAndStoreIsGrounded(FallVelocity * deltaTime);
			}
		}

		private void UpdateSliding(float deltaTime)
		{
			if(_hasStableGround)
			{
				_slidingVelocity.y = 0;
				_slidingVelocity = Vector3.Lerp(_slidingVelocity, Vector3.zero, deltaTime * _slidingStopDamping);
				_slidingVelocity = Vector3.MoveTowards(_slidingVelocity, Vector3.zero, deltaTime);
				MoveAndStoreIsGrounded(_slidingVelocity * deltaTime);
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
				MoveAndStoreIsGrounded(_slidingVelocity * deltaTime);
			}
		}

		private void MoveAndStoreIsGrounded(Vector3 vector)
		{
			UnityCharacterController.Move(vector);

			//this is required because UnityCharacterController.isGrounded is changed every time Move() called
			_isGroundedCache |= UnityCharacterController.isGrounded;
		}

		private void HandleDeath(bool isDead)
		{
			UnityCharacterController.enabled = !isDead;
		}
	}
}
