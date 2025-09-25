using game.gameplay_core.characters.ai.utility.considerations.utils;
using game.gameplay_core.characters.ai.utility.considerations.value_sources;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility
{
	public class SubUtilityFight : SubUtilityBase
	{
		[SerializeField]
		private PerlinConfig _noAttackWeight;

		//chain of goals
		//chain examples:
		//multiple attacks
		//jump back then heal
		//jump attack then roll back

		//input: enemy
		//input: self stats
		//input: inventory

		//input: fight history data

		//list of attacks
		//list of defences
		//list of movement
		//list of stupidities

		public override void Think(float deltaTime)
		{
			_context.BlackboardValues[BlackboardValues.NoAttacksWeight] = _noAttackWeight.Evaluate(_context.BrainTime);
			base.Think(deltaTime);
		}
	}
}
