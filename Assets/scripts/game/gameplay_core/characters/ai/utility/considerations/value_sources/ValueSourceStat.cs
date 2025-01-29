using System;
using dream_lib.src.reactive;
using game.gameplay_core.characters.ai.utility.blackbox;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
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
