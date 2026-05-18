using System.Collections.Generic;
using game.gameplay_core.characters.stats.config;
using Sirenix.OdinInspector;

namespace game.gameplay_core.characters.stats.runtime_data
{
	public class CharacterStatsData
	{
		public readonly Dictionary<StatKey, StatData> AllStats = new();

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
						AllStats.Add(kvp.Key, new HpStat(kvp.Key, commonStats[kvp.Key]));
						break;
					default:
						AllStats.Add(kvp.Key, new StatData(kvp.Key, commonStats[kvp.Key]));
						break;
				}
			}

			Hp = AllStats[StatKey.Hp] as HpStat;
			Stamina = AllStats[StatKey.Stamina];
			Poise = AllStats[StatKey.Poise];
			PoiseRestoreTimer = AllStats[StatKey.PoiseRestoreTimer];

			Locomotion = new LocomotionStatsData();
		}

#if UNITY_EDITOR
		[Button]
#endif

		public void SetStatsToMax()
		{
			foreach(var kvp in AllStats)
			{
				kvp.Value.SetToMax();
			}
		}

		public float GetValue(StatKey statKey)
		{
			return AllStats[statKey].Value;
		}
	}
}
