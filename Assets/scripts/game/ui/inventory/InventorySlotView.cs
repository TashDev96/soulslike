using System;
using game.enums;
using game.gameplay_core.inventory.items_logic;
using UnityEngine;
using UnityEngine.UI;

namespace game.ui.inventory
{
	public class InventorySlotView : MonoBehaviour
	{
		private const float DoubleClickThreshold = 0.3f;
		[SerializeField]
		private EquipmentSlotType _slotType;
		[SerializeField]
		private int _slotIndex;
		[SerializeField]
		private Image _iconImage;
		[SerializeField]
		private Button _button;

		private BaseItemLogic _currentItem;
		private float _lastClickTime;

		public EquipmentSlotType SlotType => _slotType;
		public int SlotIndex => _slotIndex;
		public Button Button => _button;
		public event Action<InventorySlotView> OnDoubleClick;

		private void Awake()
		{
			_button.onClick.AddListener(OnClick);
		}

		public void SetItem(BaseItemLogic item)
		{
			_currentItem = item;
			if(item != null)
			{
				_iconImage.sprite = item.BaseConfig.Icon;
				_iconImage.enabled = true;
			}
			else
			{
				_iconImage.sprite = null;
				_iconImage.enabled = false;
			}
		}

		private void OnClick()
		{
			if(Time.unscaledTime - _lastClickTime < DoubleClickThreshold)
			{
				if(_currentItem != null)
				{
					OnDoubleClick?.Invoke(this);
				}
			}
			_lastClickTime = Time.unscaledTime;
		}

		private void OnValidate()
		{
			name = $"Slot {SlotType} {SlotIndex}";
		}
	}
}
