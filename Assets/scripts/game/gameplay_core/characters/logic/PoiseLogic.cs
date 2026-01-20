using game.gameplay_core.characters.runtime_data.bindings.stats;
using game.gameplay_core.characters.state_machine.states.stagger;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.logic
{
	public class PoiseLogic
	{
		private CharacterContext _context;

		private Poise Poise => _context.CharacterStats.Poise;
		private PoiseMax PoiseMax => _context.CharacterStats.PoiseMax;
		private PoiseRestoreTimer PoiseRestoreTimer => _context.CharacterStats.PoiseRestoreTimer;

		public void SetContext(CharacterContext context)
		{
			_context = context;
			_context.ApplyDamage.OnExecute += ApplyDamage;
		}

		public void Update(float deltaTime)
		{
			if(PoiseRestoreTimer.Value > 0)
			{
				PoiseRestoreTimer.Value -= deltaTime;
				if(PoiseRestoreTimer.Value <= 0f)
				{
					Poise.Value = PoiseMax.Value;
				}
			}
		}

		public void TriggerStaggerFromBlockWithNoStamina()
		{
			ApplyStagger(StaggerReason.BlockBreak);
		}

		private void ApplyDamage(DamageInfo damageInfo)
		{
			Poise.Value -= damageInfo.PoiseDamageAmount;
			if(Poise.Value < 0)
			{
				ApplyStagger(StaggerReason.Poise);
			}

			PoiseRestoreTimer.Value = _context.CharacterStats.PoiseRestoreTimerMax.Value;
		}

		private void ApplyStagger(StaggerReason reason)
		{
			_context.TriggerStagger.Execute(reason);
			Poise.Value = PoiseMax.Value;
		}
	}
}
