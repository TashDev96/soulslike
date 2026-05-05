using System;
using dream_lib.ui;
using game.gameplay_core.inventory.items_logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace game.ui.inventory.variant_darksouls
{
	public class InventoryPossessionItemView : MonoBehaviour
	{
		private const float DoubleClickThreshold = 0.3f;
		[SerializeField]
		private UiInteractableElement _button;
		[SerializeField]
		private Image _icon;
		[SerializeField]
		private TMP_Text _nameText;
		[SerializeField]
		private TMP_Text _countText;
		[SerializeField]
		private TMP_Text _descriptionText;

		private float _lastClickTime;

		private Action<BaseItemLogic> _onDoubleClick;
		private BaseItemLogic _item;

		public void Initialize(BaseItemLogic item, Action<BaseItemLogic> onDoubleClick)
		{
			_item = item;
			_icon.sprite = item.BaseConfig.Icon;
			_onDoubleClick = onDoubleClick;
			_button.OnClick += OnClick;

			item.GetCountData(out var countAvailable, out var count);
			_countText.gameObject.SetActive(countAvailable);
			_countText.text = count.ToString();

			_nameText.text = _item.BaseConfig.name.Replace("Config", ""); // temp until localization is implemented
			_descriptionText.text = _item.BaseConfig.Description;
		}

		private void OnDestroy()
		{
			_button.OnClick -= OnClick;
		}

		private void OnClick()
		{
			if(Time.unscaledTime - _lastClickTime < DoubleClickThreshold)
			{
				_onDoubleClick?.Invoke(_item);
			}
			_lastClickTime = Time.unscaledTime;
		}
	}
}
