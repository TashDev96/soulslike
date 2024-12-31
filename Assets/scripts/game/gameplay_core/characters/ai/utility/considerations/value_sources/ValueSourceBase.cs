using System;
using game.gameplay_core.characters.ai.blackbox;

namespace game.gameplay_core.characters.ai.considerations.value_sources
{
	[Serializable]
	public abstract class ValueSourceBase
	{
		public abstract float GetValue(UtilityBrainContext context);
	}
}
