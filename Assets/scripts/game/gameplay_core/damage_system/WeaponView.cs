using System;
using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.utils;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class WeaponView : MonoBehaviour
	{
		[SerializeField]
		private CapsuleCaster[] _hitColliders;
		
		[SerializeField]
		private CapsuleCaster[] _preciseHitColliders;
		
		[SerializeField]
		private CapsuleCaster[] _handleColliders;
		
		[SerializeField]
		private BlockReceiver[] _blockColliders;
		
		public WeaponConfig Config;

		private readonly List<TransformCache> _previousCollidersPositions = new();

		private CharacterContext _context;

		public void Initialize(CharacterContext characterContext)
		{
			_context = characterContext;
			SetBlockColliderActive(false);
			
			foreach(var blockReceiver in _blockColliders)
			{
				blockReceiver.Initialize(new BlockReceiver.Context()
				{
					Team = _context.Team,
					CharacterId = _context.CharacterId,
					WeaponConfig = Config,
					BlockLogic = _context.BlockLogic,
				});
			}
		}

		public void SetBlockColliderActive(bool active)
		{
			if(_blockColliders != null)
			{
				for(var i = 0; i < _blockColliders.Length; i++)
				{
					_blockColliders[i].gameObject.SetActive(active);
				}
			}
		}

		public void CastCollidersInterpolated(WeaponColliderType colliderType, HitData hitData, Action<HitData, CapsuleCaster> handleInterpolatedCast)
		{
			var castColliders = colliderType switch
			{
				WeaponColliderType.Attack => _hitColliders,
				WeaponColliderType.PreciseContact => _preciseHitColliders,
				WeaponColliderType.Handle => _handleColliders,
				_ => _hitColliders
			};

			for(var i = 0; i < castColliders.Length; i++)
			{
				if(colliderType == WeaponColliderType.Attack && !hitData.Config.InvolvedColliders[i])
				{
					continue;
				}

				var colliderTransform = castColliders[i].transform;

				var prevRotation = Quaternion.Euler(_previousCollidersPositions[i].EulerAngles);
				var currentRotation = colliderTransform.rotation;

				var prevPosition = _previousCollidersPositions[i].Position;
				var currentPosition = colliderTransform.position;

				const float maxPosStep = 0.2f;
				const float maxAngleStep = 20f;

				var angleDiff = Quaternion.Angle(prevRotation, currentRotation);
				var posDiff = (currentPosition - prevPosition).magnitude;

				var stepsCount = Mathf.CeilToInt(Mathf.Max(angleDiff / maxAngleStep, posDiff / maxPosStep));
				for(var step = 0; step < stepsCount; step++)
				{
					var interpolationVal = (float)step / stepsCount;

					var castPos = Vector3.Lerp(prevPosition, currentPosition, interpolationVal);
					var castRotation = Quaternion.Lerp(prevRotation, currentRotation, interpolationVal);
					colliderTransform.position = castPos;
					colliderTransform.rotation = castRotation;

					handleInterpolatedCast(hitData, castColliders[i]);
				}

				colliderTransform.position = currentPosition;
				colliderTransform.rotation = currentRotation;
			}
		}

		public void CustomUpdate(float deltaTimeStep)
		{
			for(var i = 0; i < _hitColliders.Length; i++)
			{
				var hitCollider = _hitColliders[i];
				if(_previousCollidersPositions.Count <= i)
				{
					_previousCollidersPositions.Add(new TransformCache());
				}

				_previousCollidersPositions[i].Set(hitCollider.transform);
			}
		}
	}
	
	public enum WeaponColliderType {
		Attack,
		PreciseContact,
		Handle,
		Block
	}
}
