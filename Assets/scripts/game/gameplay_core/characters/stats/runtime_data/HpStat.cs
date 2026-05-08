using dream_lib.src.reactive;
using game.gameplay_core.characters.stats.config;

namespace game.gameplay_core.characters.stats.runtime_data
{
	public class HpStat : StatData
	{
		public ReactiveProperty<float> Recoverable = new();

		public HpStat(StatKey id, StatConfig config) : base(id, config)
		{
		}
	}
}
