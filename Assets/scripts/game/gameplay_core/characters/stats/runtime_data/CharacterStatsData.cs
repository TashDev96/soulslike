using System.Collections.Generic;
using game.gameplay_core.characters.stats.config;
using Sirenix.OdinInspector;

namespace game.gameplay_core.characters.stats.runtime_data
{
	public class CharacterStatsData
	{
		private readonly Dictionary<StatKey, StatData> _allStats = new();

		public HpStat Hp { get; private set; }
		public StatData Stamina { get; private set; }

		public StatData Poise { get; private set; }
		public StatData PoiseRestoreTimer { get; private set; }

		public float KnockBackMultiplier { get; set; } = 1f;

		public LocomotionStatsData Locomotion { get; private set; }

		public CharacterStatsData()
		{
			var commonStats = GameStaticContext.Instance.CommonStatsConfig.Stats;
			foreach(var kvp in commonStats)
			{
				switch(kvp.Key)
				{
					case StatKey.Hp:
						_allStats.Add(kvp.Key, new HpStat(kvp.Key, commonStats[kvp.Key]));
						break;
					default:
						_allStats.Add(kvp.Key, new StatData(kvp.Key, commonStats[kvp.Key]));
						break;
				}
			}

			Hp = _allStats[StatKey.Hp] as HpStat;
			Stamina = _allStats[StatKey.Stamina];
			Poise = _allStats[StatKey.Poise];
			PoiseRestoreTimer = _allStats[StatKey.PoiseRestoreTimer];

			Locomotion = new LocomotionStatsData();
		}

#if UNITY_EDITOR
		[Button]
#endif

		public void SetStatsToMax()
		{
			foreach(var kvp in _allStats)
			{
				kvp.Value.SetToMax();
			}
		}
	}
}
