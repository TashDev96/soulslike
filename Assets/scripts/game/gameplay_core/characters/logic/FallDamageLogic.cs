using dream_lib.src.reactive;
using game.gameplay_core.characters.state_machine.states.stagger;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class FallDamageLogic
	{
		private const float PROTECTION_COOLDOWN = 0.5f;
		private const float PROTECTION_DURATION = 1.5f;

		private const float MinimumFallDamageHeight = 8.0f;
		private const float LethalFallHeight = 18.0f;
		private const float StaggerThreshold = 5.0f;

		private readonly float _minFallDamagePercent = 0.1f;

		private readonly float _maxFallDamagePercent = 1.0f;

		private CharacterContext _context;
		private float _fallStartY;

		private float _lastProtectionActivationTime;

		public ReactiveProperty<bool> FallDamageProtectionActive { get; } = new();

		public void SetContext(CharacterContext context)
		{
			_context = context;

			_context.IsFalling.OnChangedFromTo += HandleFallingChanged;
			_lastProtectionActivationTime = -PROTECTION_COOLDOWN;
		}

		public void CustomUpdate(float deltaTime)
		{
			if(FallDamageProtectionActive.Value)
			{
				var timeSinceActivation = Time.realtimeSinceStartup - _lastProtectionActivationTime;
				if(timeSinceActivation > PROTECTION_DURATION)
				{
					FallDamageProtectionActive.Value = false;
					Debug.Log("Fall damage protection expired");
				}
			}
		}

		public bool TryActivateFallDamageProtection()
		{
			var currentTime = Time.realtimeSinceStartup;
			var timeSinceLastActivation = currentTime - _lastProtectionActivationTime;

			if(timeSinceLastActivation > PROTECTION_COOLDOWN)
			{
				FallDamageProtectionActive.Value = true;
				_lastProtectionActivationTime = currentTime;
				Debug.Log("Fall damage protection activated");
				return true;
			}

			return false;
		}

		private void HandleFallingChanged(bool wasFalling, bool isFalling)
		{
			if(isFalling && !wasFalling)
			{
				HandleStartFalling();
			}
			else if(!isFalling && wasFalling)
			{
				HandleLanded();
			}
		}

		private void HandleStartFalling()
		{
			if(!_context.IsDead.Value)
			{
				_fallStartY = _context.Transform.Position.y;
			}
		}

		private void HandleLanded()
		{
			if(!_context.IsDead.Value && !FallDamageProtectionActive.Value && !_context.InvulnerabilityLogic.IsInvulnerable)
			{
				var fallDistance = _fallStartY - _context.Transform.Position.y;

				if(fallDistance > MinimumFallDamageHeight)
				{
					var damagePercentage = Mathf.Lerp(
						_minFallDamagePercent,
						_maxFallDamagePercent,
						Mathf.InverseLerp(MinimumFallDamageHeight, LethalFallHeight, fallDistance)
					);

					var damage = damagePercentage * _context.CharacterStats.HpMax.Value;
					var staminaDamage = damagePercentage * _context.CharacterStats.StaminaMax.Value;

					var damageInfo = new DamageInfo
					{
						DamageAmount = damage,
						PoiseDamageAmount = fallDistance > StaggerThreshold ? _context.CharacterStats.PoiseMax.Value : 0f,
						WorldPos = _context.Transform.Position,
						Direction = Vector3.down,
						DoneByPlayer = false,
						DamageDealer = null
					};

					_context.ApplyDamage.Execute(damageInfo);
					_context.BodyAttackView.CastFallAttack(fallDistance);
					_context.StaminaLogic.SpendStamina(staminaDamage);

					if(fallDistance > StaggerThreshold)
					{
						_context.TriggerStagger.Execute(StaggerReason.Fall);
					}

					Debug.Log($"Fall damage applied: {damage} from height {fallDistance}m");
				}
			}
			else if(FallDamageProtectionActive.Value)
			{
				Debug.Log("Fall damage prevented by perfectly timed roll!");
			}
		}
	}
}
