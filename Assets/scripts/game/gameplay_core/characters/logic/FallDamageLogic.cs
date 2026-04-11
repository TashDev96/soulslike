using dream_lib.src.extensions;
using dream_lib.src.reactive;
using dream_lib.src.utils;
using game.gameplay_core.characters.state_machine.states.stagger;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class FallDamageLogic
	{
		private const float PROTECTION_COOLDOWN = 0.5f;
		private const float PROTECTION_DURATION = 1.5f;

		private const float MinimumFallDamageAltitude = 8.0f;
		private const float LethalFallAltitude = 18.0f;
		private const float StaggerThresholdAltitude = 5.0f;

		private float _minimumFallDamageSpeed;
		private float _lethalFallSpeed;
		private float _staggerThresholdSpeed;

		private readonly float _minFallDamagePercent = 0.1f;

		private readonly float _maxFallDamagePercent = 1.0f;

		private CharacterContext _context;
		private float _fallStartY;

		private float _lastProtectionActivationTime;

		public ReactiveProperty<bool> FallDamageProtectionActive { get; } = new();

		public void SetContext(CharacterContext context)
		{
			_context = context;

			_minimumFallDamageSpeed = CalculateFallDamageSpeed(MinimumFallDamageAltitude);
			_lethalFallSpeed = CalculateFallDamageSpeed(LethalFallAltitude);
			_staggerThresholdSpeed = CalculateFallDamageSpeed(StaggerThresholdAltitude);

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
				var fallSpeed = Mathf.Abs(Mathf.Min(0, _context.MovementLogic.FallVelocity.y));

				if(fallSpeed > _minimumFallDamageSpeed)
				{
					var damagePercentage = Mathf.LerpUnclamped(
						_minFallDamagePercent,
						_maxFallDamagePercent,
						MathUtils.InverseLerpUnclamped(_minimumFallDamageSpeed, _lethalFallSpeed, fallSpeed)
					);

					var damage = damagePercentage * _context.CharacterStats.HpMax.Value;
					var staminaDamage = damagePercentage * _context.CharacterStats.StaminaMax.Value;

					var damageInfo = new DamageInfo
					{
						DamageAmount = damage,
						PoiseDamageAmount = fallSpeed > _staggerThresholdSpeed ? _context.CharacterStats.PoiseMax.Value : 0f,
						WorldPos = _context.Transform.Position,
						Direction = Vector3.down,
						DoneByPlayer = false,
						DamageDealer = null
					};

					_context.ApplyDamage.Execute(damageInfo);
					_context.BodyAttackView.CastFallAttack(fallSpeed);
					_context.StaminaLogic.SpendStamina(staminaDamage);

					if(fallSpeed > _staggerThresholdSpeed)
					{
						_context.TriggerStagger.Execute(StaggerReason.Fall);
					}

					Debug.Log($"Fall damage applied: {damage} from speed {fallSpeed.RoundFormat(100)}m");
				}
			}
			else if(FallDamageProtectionActive.Value)
			{
				Debug.Log("Fall damage prevented by perfectly timed roll!");
			}
		}

		private float CalculateFallDamageSpeed(float altitude)
		{
			var fallTime = 0f;
			Vector3 currentPosition = default;
			Vector3 velocity = default;

			const float deltaTime = 1f / 90f;

			while(currentPosition.y > -altitude)
			{
				velocity += Physics.gravity * deltaTime;
				var dampingForce = MovementLogic.GetAirDampingForceFalling(velocity);
				velocity += dampingForce * deltaTime;

				currentPosition += velocity * deltaTime;
				fallTime += deltaTime;

				if(fallTime > 100f)
				{
					Debug.LogError("Simulation likely stuck. Possible precision issues.  Increase tolerance or check air damping settings.");
					return 10f;
				}
			}
			return velocity.magnitude;
		}
	}
}
