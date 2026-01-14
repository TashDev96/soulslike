using dream_lib.src.reactive;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.serialized_data;
using UnityEngine;

namespace game.gameplay_core.inventory.items_logic
{
	public class BaseConsumableItemLogic : BaseItemLogic, IConsumableItemLogic
	{
		private const string ChargesLeftKey = "charges_left";
		private readonly BaseConsumableItemConfig _config;
		private bool _effectApplied;

		public override BaseItemConfig BaseConfig => _config;
		public bool HasInfiniteCharges => _config.HasInfiniteCharges;
		public IReadOnlyReactiveProperty<int> ChargesLeft => _chargesLeft;
		private readonly ReactiveProperty<int> _chargesLeft = new();

		public string Id => _config.name;

		public ItemAnimationConfig AnimationConfig => _config.AnimationConfig;

		public override string ConfigId => _config.name;

		public BaseConsumableItemLogic(BaseConsumableItemConfig config)
		{
			_config = config;
		}

		public override void LoadData(InventoryItemSaveData saveData)
		{
			base.LoadData(saveData);
			if(!saveData.IsInitialized)
			{
				_chargesLeft.Value = _config.ChargesCount;
				saveData.IsInitialized = true;
				SaveData();
			}
			else
			{
				_chargesLeft.Value = SaveableData.GetInt(ChargesLeftKey);
			}
		}

		public override void SaveData()
		{
			SaveableData.SetInt(ChargesLeftKey, _chargesLeft.Value);
		}

		public bool CheckCanStartConsumption()
		{
			return HasInfiniteCharges || _chargesLeft.Value > 0;
		}

		public void HandleAnimationBegin()
		{
			_effectApplied = false;
		}

		public void HandleAnimationProgress(float normalizedTime)
		{
			if(!_effectApplied && _config.AnimationConfig.ApplyEffectTiming >= normalizedTime)
			{
				_effectApplied = true;
			}
		}

		public void HandlePickupAdditionalItem(InventoryItemSaveData itemSaveData)
		{
			if(itemSaveData.IsInitialized)
			{
				_chargesLeft.Value += itemSaveData.GetInt(ChargesLeftKey);
				SaveData();
			}
			else
			{
				Debug.LogError($"{itemSaveData} is not initialized");
			}
		}
	}
}
