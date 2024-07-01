using System;
using UnityEngine;

namespace game.gameplay_core.interactive_objects
{
	public class Bonfire : InteractiveObjectBase<BonfireSaveData>
	{
		[SerializeField]
		private int _defaultLevel;
		[SerializeField]
		private bool _unlockedByDefault;

		[SerializeField]
		private InteractionZone _interactiveZone;

		public override void InitializeFirstTime()
		{
			Data.IsUnlocked = _unlockedByDefault;
			Data.UpgradeLevel = _defaultLevel;
		}

		public override void OnDeserialize()
		{
			//set animation
		}

		protected override void HandleInteractionTriggered()
		{
			//reset level
		}

		protected override string GetInteractionTextHint()
		{
			if(Data.IsUnlocked)
			{
				return "Rest At  Bonfire";
			}
			return "Unlock Bonfire";
		}
	}

	[Serializable]
	public class BonfireSaveData
	{
		[field: SerializeField]
		public bool IsUnlocked { get; set; }
		[field: SerializeField]
		public int UpgradeLevel { get; set; }
	}
}
