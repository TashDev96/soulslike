using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.logic
{
	public class HealthLogic
	{
		private const float RecoverableFadePercentPerSecond = 50f;
		public const float RecoverableFadeDelayMax = 0.618f;

		private readonly CharacterContext _context;

		public float RecoverableFadeDelay { get; private set; }

		public HealthLogic(CharacterContext context)
		{
			_context = context;
			_context.ApplyDamage.OnExecute += ApplyDamage;
			_context.CharacterStats.Hp.OnChangedFromTo += HandleHpChanged;
			RecoverableFadeDelay = RecoverableFadeDelayMax;
		}

		public void Update(float deltaTimeStep)
		{
			if(_context.CharacterStats.RecoverableHp.Value > 0)
			{
				if(RecoverableFadeDelay > 0)
				{
					RecoverableFadeDelay -= deltaTimeStep;
				}
				else
				{
					var step = RecoverableFadePercentPerSecond * _context.CharacterStats.HpMax.Value / 100f;
					_context.CharacterStats.RecoverableHp.Value -= deltaTimeStep * step;
					if(_context.CharacterStats.RecoverableHp.Value <= 0)
					{
						_context.CharacterStats.RecoverableHp.Value = 0;
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
			_context.CharacterStats.RecoverableHp.Value -= delta;
			if(_context.CharacterStats.RecoverableHp.Value <= 0)
			{
				_context.CharacterStats.RecoverableHp.Value = 0;
			}
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
