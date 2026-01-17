using System;
using System.Collections.Generic;
using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils.components;
using dream_lib.src.utils.drawers;
using game.gameplay_core.utils;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class CapsuleCharacterCollider : CapsuleCaster
	{
		public class Context
		{
			public ReactiveHashSet<Collider> EnteredTriggers;
			public bool IsPlayer;
		}

		[SerializeField]
		private int _maxIterations = 4;
		[SerializeField]
		private float _characterToCharacterOffset = 0.5f;

		[SerializeField]
		private LayerMask _collisionMask = ~0;
		[SerializeField]
		private LayerMask _charactersCollisionMask = ~0;

		public float SkinWidth = 0.05f;
		public float SlopeLimit = 30f;
		public float StepOffset = 0.55f;
		public float MinStepOffset = 0.01f;

		[NonSerialized]
		public bool HasStableGround;

		private readonly RaycastHit[] _groundCastResults = new RaycastHit[20];

		private float _stepGravityDisableTimer;
		private readonly Collider[] _castResults = new Collider[50];

		private Context _context;
		private readonly List<Collider> _exitedTriggersCache = new();

		public bool IsGrounded => (Flags & CollisionFlags.Below) != 0 || IsFakeGrounded;
		public CollisionFlags Flags { get; private set; }
		public Vector3 GroundNormal { get; private set; } = Vector3.up;
		public bool IsOnStableSlope { get; private set; }
		public bool IsFakeGrounded => _stepGravityDisableTimer > 0;

		public void SetContext(Context context)
		{
			_context = context;
		}

		public void CustomUpdate(float deltaTime)
		{
			Flags = CollisionFlags.None;
			if(_stepGravityDisableTimer > 0)
			{
				_stepGravityDisableTimer -= deltaTime;
				Flags |= CollisionFlags.Below;
			}
		}

		public void CalculateGravity()
		{
		}

		public void Move(Vector3 motion, bool disableIterations)
		{
			var wasGrounded = IsGrounded;

			var moveStartPosition = transform.position;
			var stepUpSuccess = false;

			CalculateMovement(moveStartPosition, motion, disableIterations, out var normalResultPosition, out var normalMovementFlags);

			if(wasGrounded && !IsFakeGrounded && normalMovementFlags.HasFlag(CollisionFlags.CollidedSides))
			{
				var verticalAngle = Vector3.Angle(motion.normalized, motion.SetY(0).normalized);

				if(verticalAngle < SlopeLimit)
				{
					var stepMotion = motion + motion.normalized * SkinWidth;
					CalculateMovement(moveStartPosition + Vector3.up * StepOffset, stepMotion, disableIterations, out var resultPositionUp, out var flagsUp);
					stepUpSuccess = (moveStartPosition - resultPositionUp).SetY(0).magnitude > (moveStartPosition - normalResultPosition).SetY(0).magnitude + SkinWidth;

					DebugDrawUtils.DrawWireCapsulePersistent(resultPositionUp + Center, Height, Radius, stepUpSuccess ? Color.green : Color.red, 3f);

					if(stepUpSuccess)
					{
						CalculateMovement(resultPositionUp, Vector3.down * StepOffset, true, out var resultPositionStepGravity, out var groundingFlags);
						stepUpSuccess &= resultPositionStepGravity.y > normalResultPosition.y + MinStepOffset;

						if(stepUpSuccess)
						{
							transform.position = resultPositionStepGravity;
							Flags = flagsUp | groundingFlags;
							_stepGravityDisableTimer = 0.2f;
						}
					}
				}
			}

			if(!stepUpSuccess)
			{
				Flags |= normalMovementFlags;
				if(_stepGravityDisableTimer > 0)
				{
					Flags |= CollisionFlags.Below;
				}
				transform.position = normalResultPosition;
			}

			UpdateTriggers(transform.position);
		}

		// private void GetGroundNormal()
		// {
		// 	if(!IsGrounded)
		// 	{
		// 		GroundNormal = Vector3.up;
		// 	}
		//
		// 	var origin = transform.position + Center;
		// 	var radius = Radius + SkinWidth;
		// 	var maxDistance = Height / 2 + 0.0001f - radius + SkinWidth;
		//
		// 	var count = Physics.SphereCastNonAlloc(origin, radius, Vector3.down, _groundCastResults, maxDistance, CollisionMask);
		//
		// 	if(count == 0)
		// 	{
		// 		GroundNormal = Vector3.up;
		// 		return;
		// 	}
		//
		// 	HasStableGround = false;
		// 	var normalsSum = Vector3.zero;
		//
		// 	for(var i = 0; i < count; i++)
		// 	{
		// 		var result = _groundCastResults[i];
		// 		var angle = Vector3.Angle(Vector3.up, result.normal);
		// 		var isSliding = angle > SlopeLimit;
		// 		if(!isSliding)
		// 		{
		// 			HasStableGround = true;
		// 			GroundNormal = result.normal;
		// 		}
		//
		// 		normalsSum += result.normal;
		//
		// 		if(_drawDebug)
		// 		{
		// 			Debug.DrawRay(result.point, result.normal * 2, isSliding ? Color.red : Color.green);
		// 		}
		// 	}
		//
		// 	if(!HasStableGround)
		// 	{
		// 		GroundNormal = normalsSum / count;
		// 	}
		// }

		public bool CheckForFallOff(Vector3 moveDirection, float checkDistance, float safeDropHeight = 1.3f)
		{
			if(moveDirection.sqrMagnitude < 0.001f)
			{
				return false;
			}

			var targetPos = transform.position + moveDirection.normalized * checkDistance;

			GetCapsulePoints(targetPos, out var p1, out var p2);

			var isSafe = false;

			if(Physics.CheckCapsule(p1, p2, Radius, _collisionMask, QueryTriggerInteraction.Ignore))
			{
				isSafe = true;
			}
			else if(Physics.CapsuleCast(p1, p2, Radius, Vector3.down, out var hit, safeDropHeight + 0.1f, _collisionMask, QueryTriggerInteraction.Ignore))
			{
				isSafe = true;
			}

			return !isSafe;
		}

		public bool SampleGroundBelow(float maxDistance, out float distanceToGround)
		{
			var offset = Radius + SkinWidth;
			var radius = Radius - 0.03f;
			var origin = transform.position + Vector3.up * offset;

			var hitCount = Physics.SphereCastNonAlloc(origin, radius, Vector3.down, _groundCastResults, maxDistance + offset, _collisionMask);

			if(hitCount > 0)
			{
				distanceToGround = maxDistance + radius;
				var groundFound = false;

				for(var i = 0; i < hitCount; i++)
				{
					var iHit = _groundCastResults[i];
					if(iHit.collider.gameObject == gameObject)
					{
						continue; //skip self collider
					}

					var hitDistance = iHit.distance;
					if(hitDistance < distanceToGround)
					{
						groundFound = true;
						distanceToGround = hitDistance;
					}
				}

				if(_drawDebug)
				{
					//DebugDrawUtils.DrawHandlesSphere(origin, radius / 2, new Color(1, 0, 1, 0.3f));
					//DebugDrawUtils.DrawHandlesSphere(origin + Vector3.down * distanceToGround, radius, Color.green, 3.3f);
//
					//DebugDrawUtils.DrawText(name +" "+distanceToGround.RoundFormat(), origin + Vector3.down * (distanceToGround / 2), 10f);
				}

				return groundFound;
			}

			if(_drawDebug)
			{
				//DebugDrawUtils.DrawHandlesSphere(origin, radius / 2, new Color(1, 0, 1, 0.3f));
				//DebugDrawUtils.DrawHandlesSphere(origin + Vector3.down * maxDistance, radius, Color.red, 3.3f);
			}

			distanceToGround = 0f;
			return false;
		}

		private void UpdateTriggers(Vector3 endPos)
		{
			//it may probably skip small triggers when moving fast.
			//consider implementing CapsuleCast if last frame move vector is larger than capsule radius

			GetCapsulePoints(endPos, out var p1, out var p2);
			var count = Physics.OverlapCapsuleNonAlloc(p1, p2, Radius, _castResults, LayerMask.GetMask("Triggers"), QueryTriggerInteraction.Collide);

			for(var i = 0; i < count; i++)
			{
				if(_context.EnteredTriggers.Add(_castResults[i]))
				{
					if(_castResults[i].TryGetComponent<TriggerEventsListener>(out var listener))
					{
#if UNITY_EDITOR
						if(_context.IsPlayer)
						{
							Debug.Log($"player entered {_castResults[i]}");
						}
#endif
						listener.TriggerManualColliderEnter(gameObject);
					}
				}
			}

			foreach(var enteredTrigger in _context.EnteredTriggers)
			{
				var index = Array.IndexOf(_castResults, enteredTrigger);
				if(index < 0 || index >= count)
				{
#if UNITY_EDITOR
					if(_context.IsPlayer)
					{
						Debug.Log($"player exit {enteredTrigger}");
					}
#endif
					_exitedTriggersCache.Add(enteredTrigger);
				}
			}

			if(_exitedTriggersCache.Count > 0)
			{
				foreach(var exitedTrigger in _exitedTriggersCache)
				{
					_context.EnteredTriggers.Remove(exitedTrigger);
					if(exitedTrigger.TryGetComponent<TriggerEventsListener>(out var listener))
					{
						listener.TriggerManualColliderExit(gameObject);
					}
				}

				_exitedTriggersCache.Clear();
			}
		}

		private void CalculateMovement(Vector3 moveStartPosition, Vector3 motion, bool singleIteration, out Vector3 resultPosition, out CollisionFlags flags)
		{
			flags = CollisionFlags.None;
			resultPosition = moveStartPosition;
			var remainingMovement = motion;

			var iterations = singleIteration ? 1 : _maxIterations;

			for(var i = 0; i < iterations && remainingMovement.sqrMagnitude > 0f; i++)
			{
				var hitObstacle = CastCapsule(resultPosition, remainingMovement, out var hit);

				if(hitObstacle)
				{
					var distance = Mathf.Max(hit.distance - SkinWidth, 0f);
					resultPosition += remainingMovement.normalized * distance;
					remainingMovement -= remainingMovement.normalized * distance;

					flags |= CalculateHitFlags(hit);
					remainingMovement = CalculateRemainingMovement(hit, remainingMovement);

					if(IsGrounded)
					{
						var slopeAngle = Vector3.Angle(hit.normal, transform.up);
						IsOnStableSlope = slopeAngle <= SlopeLimit;
						GroundNormal = hit.normal;

						if(IsOnStableSlope && Vector3.Dot(remainingMovement, -transform.up) > 0)
						{
							var slopeParallel = Vector3.ProjectOnPlane(transform.up, hit.normal).normalized;
							var downhillComponent = Vector3.Dot(remainingMovement, slopeParallel);

							if(downhillComponent < 0)
							{
								remainingMovement -= slopeParallel * downhillComponent;
							}
						}
					}
				}
				else
				{
					resultPosition += remainingMovement;
					remainingMovement = Vector3.zero;
				}
			}

			CollisionFlags CalculateHitFlags(RaycastHit hit)
			{
				var flags = CollisionFlags.None;
				var upDot = Vector3.Dot(hit.normal, transform.up);
				var slopeThreshold = Mathf.Cos(SlopeLimit * Mathf.Deg2Rad);

				if(upDot > slopeThreshold)
				{
					flags |= CollisionFlags.Below;
				}
				else if(upDot < -0.707f)
				{
					flags |= CollisionFlags.Above;
				}
				else
				{
					flags |= CollisionFlags.Sides;
				}

				return flags;
			}

			Vector3 CalculateRemainingMovement(RaycastHit hit, Vector3 remaining)
			{
				return Vector3.ProjectOnPlane(remaining, hit.normal);
			}
		}

		private bool CastCapsule(Vector3 resultPosition, Vector3 remainingMovement, out RaycastHit hit)
		{
			GetCapsulePoints(resultPosition, out var p1, out var p2);
			var castDistance = remainingMovement.magnitude + SkinWidth;
			var minDist = float.MaxValue;
			hit = default;

			//cast for characters
			var count = Physics.CapsuleCastNonAlloc(p1, p2, Radius + _characterToCharacterOffset, remainingMovement.normalized, _groundCastResults, castDistance, _charactersCollisionMask, QueryTriggerInteraction.Ignore);

			for(var i = 0; i < count; i++)
			{
				var iHit = _groundCastResults[i];
				if(iHit.collider.gameObject == gameObject)
				{
					continue; //skip self collider
				}
				if(iHit.distance < minDist && Vector3.Cross(remainingMovement.normalized, iHit.normal).magnitude > 0)
				{
					minDist = iHit.distance;
					hit = iHit;
				}
			}

			//cast for geometry
			count = Physics.CapsuleCastNonAlloc(p1, p2, Radius, remainingMovement.normalized, _groundCastResults, castDistance, _collisionMask, QueryTriggerInteraction.Ignore);

			for(var i = 0; i < count; i++)
			{
				var iHit = _groundCastResults[i];
				if(iHit.distance < minDist)
				{
					minDist = iHit.distance;
					hit = iHit;
				}
			}

			if(minDist < float.MaxValue)
			{
				return true;
			}

			return false;
		}
	}
}
