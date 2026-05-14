using game.gameplay_core.characters;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.items_logic;
using game.gameplay_core.location;
using UnityEngine;
using UnityEngine.EventSystems;

namespace game.ui.inventory
{
	public class InventoryScreenView : MonoBehaviour
	{
		[SerializeField]
		private InventoryPossessionsViewAbstract _possessionsView;
		private CharacterContext _context;
		private InventorySlotView[] _slots;
		private bool _initialized;

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

			_initialized = true;
		}

		public void Refresh()
		{
			if(_context.Logic.InventoryLogic == null)
			{
				return;
			}

			foreach(var slotView in _slots)
			{
				var item = _context.Logic.InventoryLogic.GetEquipment(slotView.SlotType, slotView.SlotIndex);
				slotView.SetItem(item);
			}

			_possessionsView.Initialize(HandleItemAutoEquip);
		}

		public void ToggleIsShowing()
		{
			if(gameObject.activeSelf)
			{
				Hide();
			}
			else
			{
				Show();
			}
		}

		private void Show()
		{
			gameObject.SetActive(true);
			if(!_initialized)
			{
				Initialize(LocationStaticContext.Instance.Player.Context);
			}

			Refresh();

			var elementToSelect = _possessionsView.GetTopItemBtn() ?? _slots[0].Button;
			EventSystem.current.SetSelectedGameObject(elementToSelect.gameObject);
		}

		private void Hide()
		{
			gameObject.SetActive(false);
		}

		private void OnSlotDoubleClicked(InventorySlotView slotView)
		{
			_context.Logic.InventoryLogic.UnequipItem(slotView.SlotType, slotView.SlotIndex);
			Refresh();
		}

		private void HandleItemAutoEquip(BaseItemLogic item)
		{
			if(item.BaseConfig is BaseEquipmentItemConfig equippableConfig)
			{
				_context.Logic.InventoryLogic.EquipItemAuto(equippableConfig, item);
				Refresh();
			}
		}
	}
}
