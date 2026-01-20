using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.logic
{
	public class HealthLogic
	{
		private readonly CharacterContext _context;

		public HealthLogic(CharacterContext context)
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
