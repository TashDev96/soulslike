using System;
using game.gameplay_core.characters.ai.blackbox;

namespace game.gameplay_core.characters.ai.considerations.value_sources
{
	[Serializable]
	public abstract class ValueSourceBase
	{
		protected UtilityBrainContext _context;
		public abstract float GetValue();

	}
}
