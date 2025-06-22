using dream_lib.src.reactive;
using game.gameplay_core.characters.logic;
using game.gameplay_core.characters.runtime_data.bindings;
using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class BlockReceiver : MonoBehaviour
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<Team> Team { get; set; }
			public IReadOnlyReactiveProperty<string> CharacterId { get; set; }
			public ApplyDamageCommand ApplyDamage { get; set; }
			public InvulnerabilityLogic InvulnerabilityLogic { get; set; }
			public StaminaLogic StaminaLogic { get; set; }
			public PoiseLogic PoiseLogic { get; set; }
			public WeaponConfig WeaponConfig { get; set; }
		}

		private Context _context;

		public Team OwnerTeam => _context.Team.Value;
		public string CharacterId => _context.CharacterId.Value;

		public void Initialize(Context context)
		{
			_context = context;
		}

		public void ApplyDamage(DamageInfo damageInfo, out bool deflectAttack)
		{
			var blockStaminaCost = damageInfo.DamageAmount * (1 - _context.WeaponConfig.BlockStability / 100f);
			_context.StaminaLogic.SpendStaminaForBlock(blockStaminaCost, out var hadEnoughStamina);

			deflectAttack = false;

			if(hadEnoughStamina)
			{

				deflectAttack = _context.WeaponConfig.BlockDeflectionRating >= damageInfo.DeflectionRating;

				damageInfo.DamageAmount *= _context.WeaponConfig.DamageReduction;
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
		}
	}
}
