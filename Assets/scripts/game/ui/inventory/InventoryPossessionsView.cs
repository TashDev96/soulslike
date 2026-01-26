using System;
using System.Collections.Generic;
using System.Linq;
using game.enums;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.items_logic;
using UnityEngine;
using UnityEngine.UI;

namespace game.ui.inventory
{
	public class InventoryPossessionsView : MonoBehaviour
	{
		[SerializeField]
		private Transform _itemsContainer;
		[SerializeField]
		private InventoryPossessionItemView _itemPrefab;

		[Header("Tabs")]
		[SerializeField]
		private Button _tabAll;
		[SerializeField]
		private Button _tabWeapons;
		[SerializeField]
		private Button _tabArmor;
		[SerializeField]
		private Button _tabQuest;

		private Action<BaseItemLogic> _onItemDoubleClicked;
		private List<BaseItemLogic> _allItems = new();
		private ItemCategory _currentCategory = ItemCategory.All;

		public void Initialize(IEnumerable<BaseItemLogic> items, Action<BaseItemLogic> onItemDoubleClicked)
		{
			_allItems = items.ToList();
			_onItemDoubleClicked = onItemDoubleClicked;
			RefreshList();
		}

		private void Awake()
		{
			_tabAll.onClick.AddListener(() => SetCategory(ItemCategory.All));
			_tabWeapons.onClick.AddListener(() => SetCategory(ItemCategory.Weapons));
			_tabArmor.onClick.AddListener(() => SetCategory(ItemCategory.Armor));
			_tabQuest.onClick.AddListener(() => SetCategory(ItemCategory.Quest));
		}

		private void SetCategory(ItemCategory category)
		{
			_currentCategory = category;
			RefreshList();
		}

		private void RefreshList()
		{
			foreach(Transform child in _itemsContainer)
			{
				Destroy(child.gameObject);
			}

			var filteredItems = FilterItems(_allItems, _currentCategory);

			foreach(var item in filteredItems)
			{
				var view = Instantiate(_itemPrefab, _itemsContainer);
				view.Initialize(item, _onItemDoubleClicked);
			}
		}

		private IEnumerable<BaseItemLogic> FilterItems(IEnumerable<BaseItemLogic> items, ItemCategory category)
		{
			if(category == ItemCategory.All)
			{
				return items;
			}

			return items.Where(item =>
			{
				if(item.BaseConfig is WeaponItemConfig)
				{
					return category == ItemCategory.Weapons;
				}
				if(item.BaseConfig is ArmorItemConfig)
				{
					return category == ItemCategory.Armor;
				}

				return false;
			});
		}
	}
}
