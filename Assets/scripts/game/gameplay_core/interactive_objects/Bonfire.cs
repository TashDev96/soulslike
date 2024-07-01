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
			SaveData.IsUnlocked = _unlockedByDefault;
			SaveData.UpgradeLevel = _defaultLevel;
			InitializeAfterSaveLoaded();
		}

		protected override void InitializeAfterSaveLoaded()
		{
		}

		protected override void HandleInteractionTriggered()
		{
			//TODO reset level
			//TODO heal player
		}

		protected override string GetInteractionTextHint()
		{
			if(SaveData.IsUnlocked)
			{
				return "Rest At  Bonfire";
			}
			return "Unlock Bonfire";
		}
	}

	[Serializable]
	public class BonfireSaveData : BaseSaveData
	{
		[field: SerializeField]
		public bool IsUnlocked { get; set; }
		[field: SerializeField]
		public int UpgradeLevel { get; set; }
	}
}
