using System;
using game.gameplay_core.characters;
using game.gameplay_core.inventory.serialized_data;
using game.gameplay_core.location.interactive_objects.common;
using game.gameplay_core.location.location_save_system;
using UnityEngine;

namespace game.gameplay_core.location.interactive_objects
{
	public class PickupItem : InteractiveObjectBase<PickupItemSaveData>
	{
		[SerializeField]
		private InventoryItemSaveData _item;

		public override void InitializeFirstTime()
		{
			SaveData = new PickupItemSaveData
			{
				PickedUp = false
			};
			base.Initialize();
		}

		protected override void InitializeAfterSaveLoaded()
		{
			if(SaveData.PickedUp)
			{
				gameObject.SetActive(false);
			}
			else
			{
				base.Initialize();
			}
		}

		protected override void HandleInteractionTriggered(CharacterDomain interactedCharacter)
		{
			if(SaveData.PickedUp)
			{
				Debug.LogError("Error! trying to pick up already picked item");
				return;
			}

			SaveData.PickedUp = true;
			gameObject.SetActive(false);

			interactedCharacter.InventoryLogic.PickUpItem(_item);
		}

		protected override string GetInteractionTextHint()
		{
			return "Pick Up Item";
		}
	}

	[Serializable]
	public class PickupItemSaveData : BaseSaveData
	{
		[field: SerializeField]
		public bool PickedUp { get; set; }
	}
}
