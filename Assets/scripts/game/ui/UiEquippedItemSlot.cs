using game.gameplay_core.inventory.items_logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace game.ui
{
	public class UiEquippedItemSlot : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;
		[SerializeField]
		private TextMeshProUGUI _countText;
		[SerializeField]
		private GameObject _countRoot;

		private IConsumableItemLogic _consumableItem;

		public void SetItem(BaseItemLogic item)
		{
			if(item == null)
			{
				Clear();
				return;
			}

			_icon.sprite = item.BaseConfig.Icon;
			_icon.enabled = _icon.sprite != null;

			if(item is IConsumableItemLogic consumable)
			{
				_consumableItem = consumable;
				_countRoot.SetActive(!consumable.HasInfiniteCharges);
				UpdateCount(consumable.ChargesLeft.Value);
				consumable.ChargesLeft.OnChanged += UpdateCount;
			}
			else
			{
				_consumableItem = null;
				_countRoot.SetActive(false);
			}
		}

		public void Clear()
		{
			if(_consumableItem != null)
			{
				_consumableItem.ChargesLeft.OnChanged -= UpdateCount;
				_consumableItem = null;
			}

			_icon.sprite = null;
			_icon.enabled = false;
			_countRoot.SetActive(false);
		}

		private void UpdateCount(int count)
		{
			_countText.text = count.ToString();
		}

		private void OnDestroy()
		{
			if(_consumableItem != null)
			{
				_consumableItem.ChargesLeft.OnChanged -= UpdateCount;
			}
		}
	}
}
