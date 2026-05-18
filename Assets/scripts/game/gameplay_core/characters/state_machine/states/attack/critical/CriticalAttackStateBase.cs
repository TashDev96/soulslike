using System.Collections.Generic;
using Animancer;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.config.animation;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.characters.stats.config;
using game.gameplay_core.damage_system;
using game.gameplay_core.location;

namespace game.gameplay_core.characters.state_machine.states.attack.critical
{
	public abstract class CriticalAttackStateBase : CharacterAnimationStateBase
	{
		private const string StaminaRegenDisableKey = "CriticalAttackState";
		protected readonly CharacterDomain _target;

		private bool _staminaSpent;
		private bool _staminaRegenDisabled;
		private readonly List<HitData> _hitsData = new();

		private AttackConfig _attackConfig;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		protected abstract float LogicDamageAdd { get; }
		protected abstract float LogicDamageMultiply { get; }

		protected CriticalAttackStateBase(CharacterContext context, CharacterDomain target) : base(context)
		{
			_target = target;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			Duration = _attackConfig.Duration;

			AnimationConfig = _attackConfig.AnimationConfig;

			_staminaSpent = false;
			_hitsData.Clear();
			foreach(var hitEvent in _attackConfig.AnimationConfig.GetHitEvents())
			{
				_hitsData.Add(new HitData
				{
					Config = hitEvent
				});
			}

			_context.Views.Animator.Play(_attackConfig.AnimationConfig.Clip, 0.1f, FadeMode.FromStart);

			var camera = LocationStaticContext.Instance.CameraController;
			var lockedFlag = AnimationConfig.GetFlag(AnimationFlags.StateLocked);
			var expectedDuration = lockedFlag.EndTimeNormalized * AnimationConfig.Duration;
			camera.ShowCriticalAttackAnimation(_context.Transform, expectedDuration);

			Time = 0f;
			ResetForwardMovement();
		}

		public override void Update(float deltaTime)
		{
			base.Update(deltaTime);

			var rotationDisabled = _attackConfig.AnimationConfig.HasFlag(AnimationFlags.RotationLocked, NormalizedTime);

			if(rotationDisabled)
			{
				_context.Logic.LockOnLogic.DisableRotationForThisFrame = true;
			}
			if(_context.InputData.HasDirectionInput && !rotationDisabled)
			{
				_context.Logic.MovementLogic.RotateCharacter(_context.InputData.DirectionWorld, deltaTime);
			}

			UpdateStaminaRegenLock();

			UpdateForwardMovement(_attackConfig.ForwardMovement.Evaluate(Time), deltaTime);

			foreach(var hitData in _hitsData)
			{
				if(!hitData.IsStarted && NormalizedTime >= hitData.Config.StartTime)
				{
					hitData.IsStarted = true;
					if(!_staminaSpent)
					{
						_context.Logic.StaminaLogic.SpendStamina(_attackConfig.StaminaCost);
						_staminaSpent = true;
					}
					ApplyGuaranteedDamage(hitData);
				}

				if(hitData.IsStarted && NormalizedTime >= hitData.Config.EndTime)
				{
					hitData.IsEnded = true;
				}
			}

			if(Time >= _attackConfig.Duration)
			{
				IsComplete = true;
			}

			IsReadyToRememberNextCommand = TimeLeft < 3f;

			void UpdateStaminaRegenLock()
			{
				var disableRegen = _attackConfig.AnimationConfig.HasFlag(AnimationFlags.StaminaRegenDisabled, NormalizedTime);
				if(!_staminaRegenDisabled)
				{
					if(disableRegen)
					{
						_staminaRegenDisabled = true;
						_context.Logic.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, true);
					}
				}
				else if(!disableRegen)
				{
					_staminaRegenDisabled = false;
					_context.Logic.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, false);
				}
			}
		}

		public override void OnExit()
		{
			_context.Logic.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, false);
			base.OnExit();
		}

		public override float GetEnterStaminaCost()
		{
			return 1;
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			var stateLocked = _attackConfig.AnimationConfig.HasFlag(AnimationFlags.StateLocked, NormalizedTime);
			return !stateLocked;
		}

		protected void SetEnterParams(AttackConfig attackConfig)
		{
			_attackConfig = attackConfig;
		}

		private void ApplyGuaranteedDamage(HitData hitData)
		{
			if(_target == null)
			{
				return;
			}

			var hitConfig = hitData.Config;
			var damageAmount = (_context.CharacterStats.GetValue(StatKey.AttackDamage) + LogicDamageAdd) * hitConfig.DamageMultiplier * LogicDamageMultiply;

			var damageInfo = new DamageInfo
			{
				DamageAmount = damageAmount,
				PoiseDamageAmount = hitConfig.PoiseDamage,
				WorldPos = _target.ExternalData.Transform.Position,
				DoneByPlayer = _context.IsPlayer.Value,
				DamageDealer = _context.SelfLink,
				DeflectionRating = 0,
				KnockbackImpulse = hitConfig.KnockBackImpulse,
				Direction = _context.Transform.Forward
			};

			_target.ExternalData.ApplyDamage.Execute(damageInfo);
		}
	}
}
