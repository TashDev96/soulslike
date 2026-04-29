using System;
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
		private float _contactRadius;

		[field: SerializeField]
		public AnimationConfig TargetAnimation { get; private set; }

		public Vector3 WorldPos => _pivotBone.TransformPoint(_localOffset);

		public bool CheckPointInTriggerZone(Vector3 worldPos)
		{
			var hemispherePos = WorldPos;
			var distCheck = (worldPos - hemispherePos).sqrMagnitude < _contactRadius * _contactRadius;
			return distCheck && worldPos.y <= hemispherePos.y;
		}

		public void DrawGizmos()
		{
			if(!_drawDebug || _pivotBone == null)
			{
				return;
			}

			DebugDrawUtils.DrawWireHemisphere(WorldPos, _contactRadius, Vector3.down, Color.red);
		}
	}
}
