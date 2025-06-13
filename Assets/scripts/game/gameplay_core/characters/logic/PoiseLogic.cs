using dream_lib.src.reactive;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings;
using game.gameplay_core.characters.runtime_data.bindings.stats;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.logic
{
	public class PoiseLogic
	{
		public struct Context
		{
			public CharacterStats Stats { get; set; }
			public ApplyDamageCommand ApplyDamage { get; set; }
			public ReactiveCommand TriggerStagger { get; set; }
			public InvulnerabilityLogic InvulnerabilityLogic { get; set; }
		}

		private readonly Context _context;


		private Poise Poise => _context.Stats.Poise;
		private PoiseMax PoiseMax => _context.Stats.PoiseMax;
		private PoiseRestoreTimer PoiseRestoreTimer => _context.Stats.PoiseRestoreTimer;

		public PoiseLogic(Context context)
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

		private void ApplyDamage(DamageInfo damageInfo)
		{
			Poise.Value -= damageInfo.PoiseDamageAmount;
			if(Poise.Value < 0)
			{
				_context.TriggerStagger.Execute();
				Poise.Value = PoiseMax.Value;
			}

			PoiseRestoreTimer.Value = _context.Stats.PoiseRestoreTimerMax.Value;
		}
	}
}
