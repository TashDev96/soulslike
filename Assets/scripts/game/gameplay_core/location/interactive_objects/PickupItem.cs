using System;
using game.gameplay_core.location.interactive_objects.common;
using game.gameplay_core.location.location_save_system;
using UnityEngine;

namespace game.gameplay_core.location.interactive_objects
{
	public class PickupItem : InteractiveObjectBase<PickupItemSaveData>
	{
		[SerializeField]
		private string _itemId;

		public override void InitializeFirstTime()
		{
			SaveData = new PickupItemSaveData
			{
				PickedUp = false
			};
		}

		protected override void InitializeAfterSaveLoaded()
		{
			if(SaveData.PickedUp)
			{
				gameObject.SetActive(false);
			}
		}

		protected override void HandleInteractionTriggered()
		{
			if(SaveData.PickedUp)
			{
				Debug.LogError("Error! trying to pick up already picked item");
				return;
			}

			//TODO give item to invetoruy
			SaveData.PickedUp = true;
			gameObject.SetActive(false);

			//TODO trigger game save
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
