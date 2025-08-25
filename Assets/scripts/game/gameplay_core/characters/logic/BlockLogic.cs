using game.gameplay_core.damage_system;
using dream_lib.src.reactive;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.characters;

namespace game.gameplay_core.characters.logic
{
	public class BlockLogic
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<Team> Team { get; set; }
			public IReadOnlyReactiveProperty<string> CharacterId { get; set; }
			public ApplyDamageCommand ApplyDamage { get; set; }
			public InvulnerabilityLogic InvulnerabilityLogic { get; set; }
			public StaminaLogic StaminaLogic { get; set; }
			public PoiseLogic PoiseLogic { get; set; }
		}

		
		private readonly ReactiveCommand _onBlockTriggered = new ReactiveCommand();
		private readonly ReactiveCommand _onParryFail = new ReactiveCommand();
		
		public IReadOnlyReactiveCommand OnBlockTriggered => _onBlockTriggered;
		public IReadOnlyReactiveCommand OnParryFail => _onParryFail;
		
		private Context _context;

		public void SetContext(Context context)
		{
			_context = context;
		}

		public void ResolveBlock(DamageInfo damageInfo, WeaponConfig blockingWeapon, out bool deflectAttack)
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
