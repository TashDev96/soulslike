using System;

namespace game.gameplay_core.characters.ai.considerations.value_sources
{
	[Serializable]
	public abstract class ValueSourceBase
	{
		public CharacterContext Context;
		public abstract float GetValue();

	}
}
