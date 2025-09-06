using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.serialized_data;

namespace game.gameplay_core.inventory.items_logic
{
	public class BaseConsumableItemLogic : BaseItemLogic, IConsumableItemLogic
	{
		private const string ChargesLeftKey = "charges_left";
		private readonly BaseConsumableItemConfig _config;
		private bool _effectApplied;

		public bool HasInfiniteCharges => _config.HasInfiniteCharges;
		public int ChargesLeft { get; protected set; }

		public BaseConsumableItemLogic(BaseConsumableItemConfig config)
		{
			_config = config;
		}

		public override void LoadData(InventoryItemSaveData saveData)
		{
			base.LoadData(saveData);
			if(!saveData.IsInitialized)
			{
				ChargesLeft = _config.ChargesCount;
				saveData.IsInitialized = true;
				SaveData();
			}
			else
			{
				ChargesLeft = SaveableData.GetInt(ChargesLeftKey);
			}
		}

		public override void SaveData()
		{
			SaveableData.SetInt(ChargesLeftKey, ChargesLeft);
		}

		public bool CheckCanStartConsumption()
		{
			return HasInfiniteCharges || ChargesLeft > 0;
		}

		public ItemAnimationConfig AnimationConfig => _config.AnimationConfig;
		public void HandleAnimationBegin()
		{
			_effectApplied = false;
		}

		public void HandleAnimationProgress(float normalizedTime)
		{
			if(!_effectApplied && _config.AnimationConfig.ApplyEffectTiming>= normalizedTime)
			{
				_effectApplied = true;
			}
		}
	}
}
