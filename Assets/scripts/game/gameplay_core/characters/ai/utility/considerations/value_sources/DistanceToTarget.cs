using game.gameplay_core.characters.ai.considerations.value_sources;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	public class DistanceToTarget : ValueSourceBase
	{

		[SerializeField]
		private bool _normalizeToAttackRange = true;
		
		public override float GetValue()
		{
			var dist = Vector3.Distance(_context.TargetTransform.position, _context.CharacterContext.Transform.Position);
			if(_normalizeToAttackRange)
			{
				return dist / _context.CharacterContext.CurrentWeapon.Value.Config.RegularAttacks[0].Range;
			}
			return dist;
		}
	}
}
