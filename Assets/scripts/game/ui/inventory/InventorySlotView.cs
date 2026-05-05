using System;
using dream_lib.ui;
using game.enums;
using game.gameplay_core.inventory.items_logic;
using TMPro;
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
		private UiInteractableElement _button;
		[SerializeField]
		private TMP_Text _countText;

		private BaseItemLogic _currentItem;
		private float _lastClickTime;
		private Sprite _defaultSprite;

		public EquipmentSlotType SlotType => _slotType;
		public int SlotIndex => _slotIndex;
		public event Action<InventorySlotView> OnDoubleClick;

		private void Awake()
		{
			_button.OnClick += OnClick;
			_defaultSprite = _iconImage.sprite;
		}

		public void SetItem(BaseItemLogic item)
		{
			_currentItem = item;
			if(item != null)
			{
				_iconImage.sprite = item.BaseConfig.Icon;
				_iconImage.enabled = true;
				item.GetCountData(out var countAvailable, out var count);
				_countText.gameObject.SetActive(countAvailable);
				_countText.text = count.ToString();
			}
			else
			{
				_iconImage.sprite = _defaultSprite;
				_iconImage.enabled = false;
				_countText.gameObject.SetActive(false);
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
