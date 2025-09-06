using game.gameplay_core.inventory.item_configs;

namespace game.gameplay_core.inventory.items_logic
{
	public interface IConsumableItemLogic
	{
		public bool HasInfiniteCharges { get; }
		public int ChargesLeft { get; }
		public bool CheckCanStartConsumption();
		
		public ItemAnimationConfig AnimationConfig { get; }
		
		void HandleAnimationBegin();
		void HandleAnimationProgress(float normalizedTime);
		
	}
}
