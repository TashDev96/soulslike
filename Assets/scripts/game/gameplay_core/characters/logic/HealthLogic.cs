using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.logic
{
	public class HealthLogic
	{
		public struct Context
		{
			public ApplyDamageCommand ApplyDamage { get; set; }
			public CharacterStats CharacterStats { get; set; }
			public IsDead IsDead { get; set; }
		}

		private readonly Context _context;

		public HealthLogic(Context context)
		{
			_context = context;
			_context.ApplyDamage.OnExecute += ApplyDamage;
		}

		private void ApplyDamage(DamageInfo damageInfo)
		{
			_context.CharacterStats.Hp.Value -= damageInfo.DamageAmount;
			if(_context.CharacterStats.Hp.Value <= 0)
			{
				_context.IsDead.Value = true;
			}
		}
	}
}
