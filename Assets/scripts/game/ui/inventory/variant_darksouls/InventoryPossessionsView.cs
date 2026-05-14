using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using dream_lib.src.utils.data_types;
using dream_lib.ui;
using game.enums;
using game.gameplay_core.characters.logic;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.items_logic;
using game.gameplay_core.location;
using UnityEngine;

namespace game.ui.inventory.variant_darksouls
{
	public class InventoryPossessionsView : InventoryPossessionsViewAbstract
	{
		[SerializeField]
		private Transform _itemsContainer;
		[SerializeField]
		private InventoryPossessionItemView _itemPrefab;

		[Header("Tabs")]
		[SerializeField]
		private List<Pair<ItemCategory, UiTabButton>> _tabButtons;

		private Action<BaseItemLogic> _onItemDoubleClicked;
		private List<BaseItemLogic> _allItems = new();
		private ItemCategory _currentCategory = ItemCategory.All;
		private CharacterInventoryLogic _inventoryLogic;
		private List<InventoryPossessionItemView> _currentItemsList = new();

		public override void Initialize(Action<BaseItemLogic> autoEquipItem)
		{
			_inventoryLogic = LocationStaticContext.Instance.Player.Context.Logic.InventoryLogic;

			var equippedItemsIds = _inventoryLogic.EquippedItems.Values
				.Where(i => i != null)
				.Select(i => i.UniqueId)
				.ToHashSet();

			var unequippedItems = _inventoryLogic.GetAllItems()
				.Where(item => !equippedItemsIds.Contains(item.UniqueId));

			_allItems = unequippedItems.ToList();
			_onItemDoubleClicked = autoEquipItem;
			UniTask.DelayFrame(0, PlayerLoopTiming.LastPostLateUpdate).ContinueWith(() => { SetCategory(ItemCategory.All); }).Forget();
		}

		public override UiInteractableElement GetTopItemBtn()
		{
			return _currentItemsList.Count == 0 ? null : _currentItemsList[0].Button;
		}

		private void Awake()
		{
			foreach(var tabPair in _tabButtons)
			{
				var tabButton = tabPair.Value;
				tabButton.OnClick += () => SetCategory(tabPair.Key);
			}
		}

		private void SetCategory(ItemCategory category)
		{
			_currentCategory = category;
			foreach(var tabButton in _tabButtons)
			{
				tabButton.Value.SetActiveTab(tabButton.Key == _currentCategory);
			}
			RefreshList();
		}

		private void RefreshList()
		{
			_currentItemsList.Clear();
			foreach(Transform child in _itemsContainer)
			{
				Destroy(child.gameObject);
			}

			var filteredItems = FilterItems(_allItems, _currentCategory);

			foreach(var item in filteredItems)
			{
				var view = Instantiate(_itemPrefab, _itemsContainer);
				view.Initialize(item, _onItemDoubleClicked);
				_currentItemsList.Add(view);
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
