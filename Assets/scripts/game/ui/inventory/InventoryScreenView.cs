using System.Linq;
using game.gameplay_core;
using game.gameplay_core.characters;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.items_logic;
using UnityEngine;

namespace game.ui.inventory
{
	public class InventoryScreenView : MonoBehaviour
	{
		[SerializeField]
		private InventoryPossessionsViewAbstract _possessionsView;
		private CharacterContext _context;
		private InventorySlotView[] _slots;

		public void Initialize(CharacterContext context)
		{
			_context = context;
			if(_slots != null)
			{
				foreach(var slot in _slots)
				{
					slot.OnDoubleClick -= OnSlotDoubleClicked;
				}
			}

			_slots = GetComponentsInChildren<InventorySlotView>(true);

			foreach(var slot in _slots)
			{
				slot.OnDoubleClick += OnSlotDoubleClicked;
			}

			Refresh();
		}

		public void Refresh()
		{
			if(_context.InventoryLogic == null)
			{
				return;
			}

			foreach(var slotView in _slots)
			{
				var item = _context.InventoryLogic.GetEquipment(slotView.SlotType, slotView.SlotIndex);
				slotView.SetItem(item);
			}

		

			_possessionsView.Initialize(HandleItemAutoEquip);
		}

		public void ToggleIsShowing()
		{
			if(gameObject.activeSelf)
			{
				gameObject.SetActive(false);
			}
			else
			{
				gameObject.SetActive(true);
				Initialize(LocationStaticContext.Instance.Player.Context);
			}
		}

		private void OnSlotDoubleClicked(InventorySlotView slotView)
		{
			_context.InventoryLogic.UnequipItem(slotView.SlotType, slotView.SlotIndex);
			Refresh();
		}

		private void HandleItemAutoEquip(BaseItemLogic item)
		{
			if(item.BaseConfig is BaseEquipmentItemConfig equippableConfig)
			{
				_context.InventoryLogic.EquipItemAuto(equippableConfig, item);
				Refresh();
			}
		}
	}
}
