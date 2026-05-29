using System;
using UnityEngine;

namespace game.gameplay_core.characters.config
{
	[Serializable]
	public class DebugVars
	{
		public bool DisableFall;
		[field: SerializeField]
		public bool Single { get; set; }
		public bool IsFallCall { get; set; }
	}
}
