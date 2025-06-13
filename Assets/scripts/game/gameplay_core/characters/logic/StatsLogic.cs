using game.gameplay_core.characters.config;
using game.gameplay_core.characters.runtime_data;

namespace game.gameplay_core.characters.logic
{
	public class StatsLogic
	{
		public struct Context
		{
			public CharacterStats CharacterStats;
			public CharacterConfig CharacterConfig;
		}

		private readonly Context _context;

		public StatsLogic(Context context)
		{
			_context = context;
			InitializeStats();
		}

		private void InitializeStats()
		{
			var baseStats = _context.CharacterConfig.BaseStats;
			
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