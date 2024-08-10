using dream_lib.src.reactive;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.runtime_data.bindings.stats;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.logic
{
	public class StaggerLogic
	{
		private readonly Context _context;

		private float _poiseRestoreTimer = 0f;

		private Poise Poise => _context.Stats.Poise;
		private PoiseMax PoiseMax => _context.Stats.PoiseMax;

		public struct Context
		{
			public CharacterStats Stats { get; set; }
			public ReactiveCommand<DamageInfo> ApplyDamage { get; set; }
			public ReactiveCommand TriggerStagger { get; set; }
		}

		public StaggerLogic(Context context)
		{
			_context = context;
			_context.ApplyDamage.OnExecute += ApplyDamage;
		}

		private void ApplyDamage(DamageInfo damageInfo)
		{
			Poise.Value -= damageInfo.PoiseDamageAmount;
			if(Poise.Value < 0)
			{
				_context.TriggerStagger.Execute();
				Poise.Value = PoiseMax.Value;
			}
			
			_poiseRestoreTimer = _context.Stats.PoiseRestoreTime.Value;
		}

		public void CustomUpdate(float deltaTime)
		{
			if(_poiseRestoreTimer > 0)
			{
				_poiseRestoreTimer -= deltaTime;
				if(_poiseRestoreTimer <= 0f)
				{
					Poise.Value = PoiseMax.Value;
				}
			}
		}
	}
}
