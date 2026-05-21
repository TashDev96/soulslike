using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class HealthLogic
	{
		private const float RecoverableFadePercentPerSecond = 50f;
		public const float RecoverableFadeDelayMax = 0.618f;

		private CharacterContext _context;

		public float RecoverableFadeDelay { get; private set; }

		public void SetContext(CharacterContext context)
		{
			_context = context;
			_context.Events.ApplyDamage.OnExecute += ApplyDamage;
			_context.CharacterStats.Hp.Current.OnChangedFromTo += HandleHpChanged;
			RecoverableFadeDelay = RecoverableFadeDelayMax;
		}

		public void Update(float deltaTimeStep)
		{
			if(_context.CharacterStats.Hp.Recoverable.Value > 0)
			{
				if(RecoverableFadeDelay > 0)
				{
					RecoverableFadeDelay -= deltaTimeStep;
				}
				else
				{
					var step = RecoverableFadePercentPerSecond * _context.CharacterStats.Hp.MaxValue / 100f;
					_context.CharacterStats.Hp.Recoverable.Value -= deltaTimeStep * step;
					if(_context.CharacterStats.Hp.Recoverable.Value <= 0)
					{
						_context.CharacterStats.Hp.Recoverable.Value = 0;
						RecoverableFadeDelay = RecoverableFadeDelayMax;
					}
				}
			}
			else
			{
				RecoverableFadeDelay = RecoverableFadeDelayMax;
			}
		}

		private void HandleHpChanged(float oldValue, float newValue)
		{
			var delta = newValue - oldValue;
			_context.CharacterStats.Hp.Recoverable.Value -= delta;
			if(_context.CharacterStats.Hp.Recoverable.Value <= 0)
			{
				_context.CharacterStats.Hp.Recoverable.Value = 0;
			}
		}

		private void ApplyDamage(DamageInfo damageInfo)
		{
			if(damageInfo.DamageAmount < 0.1f)
			{
				damageInfo.DamageAmount = 0;
			}
			else if(damageInfo.DamageAmount < 1)
			{
				damageInfo.DamageAmount = 1;
			}
			damageInfo.DamageAmount = Mathf.Ceil(damageInfo.DamageAmount);

			_context.CharacterStats.Hp.Value -= damageInfo.DamageAmount;
			if(_context.CharacterStats.Hp.Value <= 0)
			{
				_context.IsDead.Value = true;
			}
		}
	}
}
