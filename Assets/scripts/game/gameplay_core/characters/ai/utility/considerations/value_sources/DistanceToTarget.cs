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
			var dist = context.BlackboardValues[BlackboardValues.DistanceToTarget];
			if(_normalizeToAttackRange)
			{
				return dist / context.BlackboardValues[BlackboardValues.BasicAttackRange];
			}
			return dist;
		}
	}
}
