using System;
using game.gameplay_core.characters.ai.utility.blackbox;

namespace game.gameplay_core.characters.ai.utility.considerations.value_sources
{
	[Serializable]
	public abstract class ValueSourceBase
	{
		public abstract float GetValue(UtilityBrainContext context);
	}
}
