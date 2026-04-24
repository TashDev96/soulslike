namespace game.gameplay_core.characters.logic
{
	public class CharacterStatsLogic
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
			var data = _context.CharacterStats;

			data.HpMax.Value = baseStats.HpMax;
			data.Hp.Value = baseStats.HpMax;

			data.StaminaMax.Value = baseStats.StaminaMax;
			data.Stamina.Value = baseStats.StaminaMax;

			data.PoiseMax.Value = baseStats.PoiseMax;
			data.Poise.Value = baseStats.PoiseMax;

			data.PoiseRestoreTimerMax.Value = baseStats.PoiseRestoreTimerMax;
			data.PoiseRestoreTimer.Value = 0f;

			data.Locomotion.HalfTurnDurationSeconds = _context.Config.Locomotion.HalfTurnDurationSeconds;
			data.Locomotion.HalfTurnDurationSecondsLockOn = _context.Config.Locomotion.HalfTurnDurationSecondsLockOn;
			data.Locomotion.RunSpeed = _context.Config.Locomotion.RunSpeed;
			data.Locomotion.WalkAcceleration = _context.Config.Locomotion.WalkAcceleration;
			data.Locomotion.WalkDeceleration = _context.Config.Locomotion.WalkDeceleration;
			data.Locomotion.WalkSpeed = _context.Config.Locomotion.WalkSpeed;
		}

		//TODO: solid stats increment logic, with multipliers paired with string ids or smh
	}
}
