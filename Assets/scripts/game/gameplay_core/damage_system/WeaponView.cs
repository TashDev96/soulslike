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
		public WeaponConfig Config;

		private readonly List<TransformCache> _previousCollidersPositions = new();

		private CharacterContext _context;

		public void Initialize(CharacterContext characterContext)
		{
			_context = characterContext;
		}

		public void CastAttackInterpolated(AttackConfig attackConfig, HitData hitData)
		{
			for(var i = 0; i < _hitColliders.Length; i++)
			{
				if(!hitData.Config.InvolvedColliders[i])
				{
					continue;
				}

				var colliderTransform = _hitColliders[i].transform;

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

					AttackHelpers.CastAttack(attackConfig.BaseDamage, hitData, _hitColliders[i], _context, true);
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
}
