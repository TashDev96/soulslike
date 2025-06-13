using System;
using UnityEngine;

namespace game.gameplay_core.characters.runtime_data
{
	[Serializable]
	public class BaseStats
	{
		[SerializeField]
		public float HpMax;
		[SerializeField]
		public float StaminaMax;
		[SerializeField]
		public float PoiseMax;
		[SerializeField]
		public float PoiseRestoreTimerMax;
	}
} 