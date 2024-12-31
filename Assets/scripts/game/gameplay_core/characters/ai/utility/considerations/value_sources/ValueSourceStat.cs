using System;
using dream_lib.src.reactive;
using game.gameplay_core.characters.ai.blackbox;
using game.gameplay_core.characters.ai.considerations.value_sources;

namespace game.gameplay_core.characters.ai.considerations
{
	[Serializable]
	public class ValueSourceStat : ValueSourceBase
	{
		public Stats Stat;

		//evaluate stat
		public override float GetValue(UtilityBrainContext context)
		{
			var stat = GetStat(context);
			return stat.Value;
		}

		private IReadOnlyReactiveProperty<float> GetStat(UtilityBrainContext context)
		{
			switch(Stat)
			{
				case Stats.HpPercent:
					return context.CharacterContext.CharacterStats.Hp;
				case Stats.Stamina:
					return context.CharacterContext.CharacterStats.Stamina;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	public enum Stats
	{
		HpPercent,
		Stamina
	}
}
