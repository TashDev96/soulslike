using System;
using dream_lib.src.reactive;
using game.gameplay_core.characters.ai.considerations.value_sources;

namespace game.gameplay_core.characters.ai.considerations
{
	[Serializable]
	public class ValueSourceStat : ValueSourceBase
	{
		public Stats Stat;

		//evaluate stat
		public override float GetValue()
		{
			var stat = GetStat();
			return stat.Value;
		}

		private IReadOnlyReactiveProperty<float> GetStat()
		{
			switch(Stat)
			{
				case Stats.HpPercent:
					return Context.CharacterStats.Hp;
				case Stats.Stamina:
					return Context.CharacterStats.Stamina;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	public enum Stats
	{
		HpPercent,
		Stamina,
	}
}
