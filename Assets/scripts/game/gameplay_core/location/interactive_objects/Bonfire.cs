using System;
using game.gameplay_core.characters;
using game.gameplay_core.location.interactive_objects.common;
using game.gameplay_core.location.location_save_system;
using UnityEngine;

namespace game.gameplay_core.location.interactive_objects
{
	public class Bonfire : InteractiveObjectBase<BonfireSaveData>
	{
		[SerializeField]
		private int _defaultLevel;
		[SerializeField]
		private bool _unlockedByDefault;

		public void SetContext(CharacterDomain locationContextPlayer)
		{
			base.Initialize();
		}

		public override void InitializeFirstTime()
		{
			SaveData = new BonfireSaveData
			{
				IsUnlocked = _unlockedByDefault,
				UpgradeLevel = _defaultLevel
			};
			InitializeAfterSaveLoaded();
		}

		protected override void InitializeAfterSaveLoaded()
		{
			base.Initialize();
		}

		protected override void HandleInteractionTriggered(CharacterDomain interactingCharacter)
		{
			//TODO reset level
			//TODO heal player
		}

		protected override string GetInteractionTextHint()
		{
			if(SaveData.IsUnlocked)
			{
				return "Rest At Bonfire";
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
