using System;
using dream_lib.src.utils.data_types;
using dream_lib.src.utils.drawers;
using game.gameplay_core.characters.config.animation;
using UnityEngine;

namespace game.gameplay_core.characters.view
{
	[Serializable]
	public class PlungeAttackTargetView
	{
		[SerializeField]
		private bool _drawDebug;

		[SerializeField]
		private Transform _pivotBone;
		[SerializeField]
		private Vector3 _localOffset;
		[SerializeField]
		private Vector3 _localOffsetAttacker;
		[SerializeField]
		private Quaternion _localRotation;

		[SerializeField]
		private float _contactRadius;

		[SerializeField]
		private Vector3 _attackerOutSpeedLocal;
		[SerializeField]
		private float _anyRotationModeOffset;

		private Quaternion _overrideAttackerLocalRotation;

		[field: SerializeField]
		public bool AnyRotationMode { get; private set; }

		[field: SerializeField]
		public AnimationConfig TargetAnimation { get; private set; }

		public Vector3 WorldPos => _pivotBone.TransformPoint(_localOffset);

		public Vector3 AttackerOutSpeed => _pivotBone.transform.TransformVector(_attackerOutSpeedLocal);

		public Vector3 GetWorldPosAttacker()
		{
			if(AnyRotationMode)
			{
				return _pivotBone.TransformPoint(_localOffsetAttacker) - GetWorldRotationAttacker() * (Vector3.forward * _anyRotationModeOffset);
			}
			return _pivotBone.TransformPoint(_localOffsetAttacker);
		}

		public bool CheckPointInTriggerZone(Vector3 worldPos)
		{
			var hemispherePos = WorldPos;
			var distCheck = (worldPos - hemispherePos).sqrMagnitude < _contactRadius * _contactRadius;
			return distCheck && worldPos.y <= hemispherePos.y;
		}

		public void SetAttackerLocalRotation(CharacterTransform attackerTransform)
		{
			var worldUp = _pivotBone.rotation * _localRotation * Vector3.up;
			var worldRotation = Quaternion.LookRotation(WorldPos - attackerTransform.Position, worldUp);
			_overrideAttackerLocalRotation = worldRotation * Quaternion.Inverse(_pivotBone.rotation);
		}

		public Quaternion GetWorldRotationAttacker()
		{
			if(AnyRotationMode)
			{
				return _pivotBone.rotation * _overrideAttackerLocalRotation;
			}
			return _pivotBone.rotation * _localRotation;
		}

		public void DrawGizmos()
		{
			if(!_drawDebug || _pivotBone == null)
			{
				return;
			}

			DebugDrawUtils.DrawArrow(GetWorldPosAttacker(), GetWorldPosAttacker() + _pivotBone.rotation * _localRotation * Vector3.forward, Color.red);
			DebugDrawUtils.DrawArrow(GetWorldPosAttacker(), GetWorldPosAttacker() + _pivotBone.TransformVector(_attackerOutSpeedLocal), Color.green);
			DebugDrawUtils.DrawWireHemisphere(WorldPos, _contactRadius, Vector3.down, Color.red);
		}
	}
}
