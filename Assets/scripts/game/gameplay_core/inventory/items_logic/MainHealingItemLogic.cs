using game.gameplay_core.characters;
using game.gameplay_core.inventory.item_configs;
using game.gameplay_core.inventory.serialized_data;

namespace game.gameplay_core.inventory.items_logic
{
	public class MainHealingItemLogic : BaseItemLogic, IConsumableItemLogic
	{
		private const string ChargesLeftKey = "charges_left";

		private readonly MainHealingItemConfig _config;
		private float _healProgressDone;
		private CharacterContext _characterContext;

		public int ChargesLeft { get; private set; }
		public float HealAmount => _config.BaseHealingAmount;

		public bool HasInfiniteCharges => false;

		public ItemAnimationConfig AnimationConfig => _config.AnimationConfig;

		public MainHealingItemLogic(MainHealingItemConfig config)
		{
			_config = config;
		}

		public override void InitializeForLocation(CharacterContext context)
		{
			base.InitializeForLocation(context);
			_characterContext = context;
		}

		public void HandleAnimationBegin()
		{
			_healProgressDone = 0f;
		}

		public void HandleAnimationProgress(float normalizedTime)
		{
			var healProgress = _config.HealingOverTime.Evaluate(normalizedTime);
			if(healProgress > _healProgressDone)
			{
				var healNormalizedDelta = healProgress - _healProgressDone;
				_characterContext.CharacterStats.Hp.Value += healNormalizedDelta * HealAmount;
				if(_characterContext.CharacterStats.Hp.Value > _characterContext.CharacterStats.HpMax.Value)
				{
					_characterContext.CharacterStats.Hp.Value = _characterContext.CharacterStats.HpMax.Value;
				}
				_healProgressDone = healProgress;
			}
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
			return ChargesLeft > 0;
		}
	}
}
