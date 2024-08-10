using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.state_machine;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class WeaponView : MonoBehaviour
	{
		[SerializeField]
		private CapsuleCollider[] _hitColliders;
		public WeaponConfig Config;

		private readonly List<TransformCache> _previousCollidersPositions = new();

		private int _layerMask;
		private readonly Collider[] _results = new Collider[40];
		private CharacterContext _context;

		public void Initialize(CharacterContext characterContext)
		{
			_context = characterContext;
			_layerMask = LayerMask.GetMask("DamageReceivers");
		}

		public void CastAttack(AttackConfig attackConfig, HitData hitData)
		{
			for(var i = 0; i < _hitColliders.Length; i++)
			{
				if(hitData.Config.InvolvedColliders[i])
				{
					var hitCollider = _hitColliders[i];
					var colliderTransform = hitCollider.transform;

					var prevRotation = Quaternion.Euler(_previousCollidersPositions[i].EulerAngles);
					var currentRotation = colliderTransform.rotation;

					var prevPosition = _previousCollidersPositions[i].Position;
					var currentPosition = colliderTransform.position;

					var radius = _hitColliders[i].radius;
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

						var point0 = colliderTransform.TransformPoint(new Vector3(0, hitCollider.height / 2 - radius, 0));
						var point1 = colliderTransform.TransformPoint(new Vector3(0, -hitCollider.height / 2 + radius, 0));
						Debug.DrawLine(point0, point1, Color.red * new Vector4(interpolationVal, interpolationVal, interpolationVal, 1), 4f);

						var count = Physics.OverlapCapsuleNonAlloc(point0, point1, radius, _results, _layerMask);
						for(var j = 0; j < count; j++)
						{
							if(hitData.ImpactedColliders.Contains(_results[j]))
							{
								continue;
							}
							hitData.ImpactedColliders.Add(_results[j]);
							var damageReceiver = _results[j].GetComponent<DamageReceiver>();
							if(damageReceiver != null)
							{
								if(damageReceiver.OwnerTeam == _context.Team.Value && !hitData.Config.FriendlyFire)
								{
									continue;
								}

								if(hitData.ImpactedCharacters.Contains(damageReceiver.CharacterId) || damageReceiver.CharacterId == _context.CharacterId.Value)
								{
									continue;
								}

								hitData.ImpactedCharacters.Add(damageReceiver.CharacterId);

								damageReceiver.ApplyDamage(new DamageInfo
								{
									DamageAmount = attackConfig.BaseDamage * hitData.Config.DamageMultiplier,
									WorldPos = Vector3.Lerp((point0 + point1) / 2f, _results[j].transform.position, 0.3f),
									DoneByPlayer = _context.IsPlayer.Value,
								});
							}
						}
					}

					colliderTransform.position = currentPosition;
					colliderTransform.rotation = currentRotation;
				}
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
