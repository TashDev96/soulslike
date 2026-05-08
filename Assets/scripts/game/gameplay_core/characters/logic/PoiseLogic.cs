using dream_lib.src.reactive;
using game.gameplay_core.characters.state_machine.states.stagger;
using game.gameplay_core.characters.stats.runtime_data;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.logic
{
	public class PoiseLogic
	{
		private CharacterContext _context;

		private StatData Poise => _context.CharacterStats.Poise;
		private ReactiveProperty<float> PoiseRestoreTimer => _context.CharacterStats.PoiseRestoreTimer.Current;

		public void SetContext(CharacterContext context)
		{
			_context = context;
			_context.Events.ApplyDamage.OnExecute += ApplyDamage;
		}

		public void Update(float deltaTime)
		{
			if(PoiseRestoreTimer.Value > 0)
			{
				PoiseRestoreTimer.Value -= deltaTime;
				if(PoiseRestoreTimer.Value <= 0f)
				{
					Poise.SetToMax();
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

			PoiseRestoreTimer.Value = _context.CharacterStats.PoiseRestoreTimer.MaxValue;
		}

		private void ApplyStagger(StaggerReason reason)
		{
			_context.Events.TriggerStagger.Execute(reason);
			Poise.SetToMax();
		}
	}
}
