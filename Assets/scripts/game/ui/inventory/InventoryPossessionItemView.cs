using System;
using game.gameplay_core.inventory.items_logic;
using UnityEngine;
using UnityEngine.UI;

namespace game.ui.inventory
{
	public class InventoryPossessionItemView : MonoBehaviour
	{
		private const float DoubleClickThreshold = 0.3f;
		[SerializeField]
		private Button _button;
		[SerializeField]
		private Image _icon;

		private float _lastClickTime;

		private Action<BaseItemLogic> _onDoubleClick;
		private BaseItemLogic _item;

		public void Initialize(BaseItemLogic item, Action<BaseItemLogic> onDoubleClick)
		{
			_item = item;
			_icon.sprite = item.BaseConfig.Icon;
			_onDoubleClick = onDoubleClick;
			_button.onClick.AddListener(OnClick);
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
