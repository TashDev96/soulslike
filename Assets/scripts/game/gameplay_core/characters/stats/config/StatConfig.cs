using System;
using UnityEngine;

namespace game.gameplay_core.characters.stats.config
{
	[Serializable]
	public class StatConfig
	{
		[field: SerializeField]
		public Sprite Icon { get; private set; }

		[field: SerializeField]
		public StatType Type { get; private set; }

		[field: SerializeField]
		public bool IsHidden { get; private set; }

		[field: SerializeField]
		public int DefaultValue { get; private set; } = 1;
	}
}
