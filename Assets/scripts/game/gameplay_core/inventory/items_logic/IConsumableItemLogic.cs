using game.gameplay_core.inventory.item_configs;

namespace game.gameplay_core.inventory.items_logic
{
	public interface IConsumableItemLogic
	{
		public bool HasInfiniteCharges { get; }
		public int ChargesLeft { get; }

		public ItemAnimationConfig AnimationConfig { get; }
		public bool CheckCanStartConsumption();

		void HandleAnimationBegin();
		void HandleAnimationProgress(float normalizedTime);
	}
}
