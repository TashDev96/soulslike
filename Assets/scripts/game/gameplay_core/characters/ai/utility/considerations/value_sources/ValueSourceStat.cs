using System;
using dream_lib.src.extensions;
using game.gameplay_core.characters.ai.utility.blackbox;
using game.gameplay_core.characters.stats.config;
using Sirenix.OdinInspector;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	[Serializable]
	public class ValueSourceStat : ValueSourceBase
	{
		public StatKey StatKey;

		public bool Normalized;

		[ShowInInspector, ReadOnly]
		private string _debugStatValue;

		//evaluate stat
		public override float GetValue(UtilityBrainContext context)
		{
			var stat = context.CharacterContext.CharacterStats.AllStats[StatKey];


			var result = Normalized ? stat.Value/stat.MaxValue : stat.Value;
			 
#if UNITY_EDITOR
			_debugStatValue = result.RoundFormat()+"   "+ stat.ToString();
#endif
			
			return result;
		}
	}
}
