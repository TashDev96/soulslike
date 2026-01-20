using dream_lib.src.reactive;
using game.gameplay_core.damage_system;
using game.gameplay_core.inventory.item_configs;

namespace game.gameplay_core.characters.logic
{
	public class BlockLogic
	{
		private readonly ReactiveCommand _onBlockTriggered = new();
		private readonly ReactiveCommand _onParryFail = new();

		private CharacterContext _context;

		public IReadOnlyReactiveCommand OnBlockTriggered => _onBlockTriggered;
		public IReadOnlyReactiveCommand OnParryFail => _onParryFail;

		public void SetContext(CharacterContext context)
		{
			_context = context;
		}

		public void ResolveBlock(DamageInfo damageInfo, WeaponItemConfig blockingWeapon, out bool deflectAttack)
		{
			var blockStaminaCost = damageInfo.DamageAmount * (1 - blockingWeapon.BlockStability / 100f);
			_context.StaminaLogic.SpendStaminaForBlock(blockStaminaCost, out var hadEnoughStamina);

			deflectAttack = false;

			if(hadEnoughStamina)
			{
				deflectAttack = blockingWeapon.BlockDeflectionRating >= damageInfo.DeflectionRating;

				damageInfo.DamageAmount *= blockingWeapon.DamageReduction;
			}
			else
			{
				_context.PoiseLogic.TriggerStaggerFromBlockWithNoStamina();
			}

			if(_context.InvulnerabilityLogic.IsInvulnerable)
			{
				return;
			}

			damageInfo.PoiseDamageAmount = 0;

			_context.ApplyDamage.Execute(damageInfo);
			_onBlockTriggered.Execute();
		}
	}
}
