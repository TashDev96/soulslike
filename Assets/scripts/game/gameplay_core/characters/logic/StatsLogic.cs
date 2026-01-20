namespace game.gameplay_core.characters.logic
{
	public class StatsLogic
	{
		private CharacterContext _context;

		public void SetContext(CharacterContext context)
		{
			_context = context;
			InitializeStats();
		}

		private void InitializeStats()
		{
			var baseStats = _context.Config.BaseStats;

			_context.CharacterStats.HpMax.Value = baseStats.HpMax;
			_context.CharacterStats.Hp.Value = baseStats.HpMax;

			_context.CharacterStats.StaminaMax.Value = baseStats.StaminaMax;
			_context.CharacterStats.Stamina.Value = baseStats.StaminaMax;

			_context.CharacterStats.PoiseMax.Value = baseStats.PoiseMax;
			_context.CharacterStats.Poise.Value = baseStats.PoiseMax;

			_context.CharacterStats.PoiseRestoreTimerMax.Value = baseStats.PoiseRestoreTimerMax;
			_context.CharacterStats.PoiseRestoreTimer.Value = 0f;
		}
	}
}
