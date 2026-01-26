using game.enums;
using game.gameplay_core.characters;
using game.gameplay_core.inventory.items_logic;
using UnityEngine;

namespace game.ui
{
	public class UiEquippedItems : MonoBehaviour
	{
		public struct Context
		{
			public CharacterDomain Player;
		}

		[SerializeField]
		private UiEquippedItemSlot _leftSlot;
		[SerializeField]
		private UiEquippedItemSlot _rightSlot;
		[SerializeField]
		private UiEquippedItemSlot _consumableSlot;

		private Context _context;

		public void SetContext(Context context)
		{
			_context = context;

			var inventory = _context.Player.ExternalData.InventoryLogic;

			_leftSlot.SetItem(inventory.GetEquipment(EquipmentSlotType.LeftHand));
			_rightSlot.SetItem(inventory.GetEquipment(EquipmentSlotType.RightHand));

			UpdateConsumable(_context.Player.Context.CurrentConsumableItem.Value);
			_context.Player.Context.CurrentConsumableItem.OnChanged += UpdateConsumable;
		}

		private void UpdateConsumable(IConsumableItemLogic item)
		{
			_consumableSlot.SetItem(item as BaseItemLogic);
		}

		private void OnDestroy()
		{
			if(_context.Player != null)
			{
				_context.Player.Context.CurrentConsumableItem.OnChanged -= UpdateConsumable;
			}
		}
	}
}
