using game.gameplay_core.characters.ai.utility.blackbox;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	public class DistanceToTarget : ValueSourceBase
	{
		[SerializeField]
		private bool _normalizeToAttackRange = true;

		public override float GetValue(UtilityBrainContext context)
		{
			var dist = Vector3.Distance(context.TargetTransform.Position, context.CharacterContext.Transform.Position);
			if(_normalizeToAttackRange)
			{
				return dist / context.CharacterContext.CurrentWeapon.Value.Config.RegularAttacks[0].Range;
			}
			return dist;
		}
	}
}
