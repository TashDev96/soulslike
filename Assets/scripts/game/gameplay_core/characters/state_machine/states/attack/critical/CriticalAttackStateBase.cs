using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states.attack.critical
{
	public class CriticalAttackStateBase : CharacterAnimationStateBase
	{
		private const string StaminaRegenDisableKey = "CriticalAttackState";

		private bool _staminaSpent;
		private bool _staminaRegenDisabled;
		private readonly List<HitData> _hitsData = new();
		protected readonly CharacterDomain _target;

		private AttackConfig _attackConfig;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public CriticalAttackStateBase(CharacterContext context, CharacterDomain target) : base(context)
		{
			_target = target;
		}

		public void SetEnterParams(AttackConfig attackConfig)
		{
			_attackConfig = attackConfig;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			Duration = _attackConfig.Duration;

			_staminaSpent = false;
			_hitsData.Clear();
			for(var i = 0; i < _attackConfig.HitConfigs.Count; i++)
			{
				_hitsData.Add(new HitData
				{
					Config = _attackConfig.HitConfigs[i]
				});
			}

			_context.Animator.Play(_attackConfig.Animation, 0.1f, FadeMode.FromStart);

			Time = 0f;
			ResetForwardMovement();

		}

		public override void Update(float deltaTime)
		{
			Time += deltaTime;

			if(_context.InputData.HasDirectionInput && !_attackConfig.RotationDisabledTime.Contains(NormalizedTime))
			{
				_context.MovementLogic.RotateCharacter(_context.InputData.DirectionWorld, deltaTime);
			}

			UpdateStaminaRegenLock();

			UpdateForwardMovement(_attackConfig.ForwardMovement.Evaluate(Time), deltaTime);

			foreach(var hitData in _hitsData)
			{
				var hitTiming = hitData.Config.Timing;

				if(!hitData.IsStarted && NormalizedTime >= hitTiming.x)
				{
					hitData.IsStarted = true;
					if(!_staminaSpent)
					{
						_context.StaminaLogic.SpendStamina(_attackConfig.StaminaCost);
						_staminaSpent = true;
					}
					ApplyGuaranteedDamage(hitData);
				}

				if(hitData.IsStarted && NormalizedTime >= hitTiming.y)
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
				if(!_staminaRegenDisabled)
				{
					if(_attackConfig.StaminaRegenDisabledTime.Contains(NormalizedTime))
					{
						_staminaRegenDisabled = true;
						_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, true);
					}
				}
				else if(!_attackConfig.StaminaRegenDisabledTime.Contains(NormalizedTime))
				{
					_staminaRegenDisabled = false;
					_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, false);
				}
			}
		}

		public override void OnExit()
		{
			_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, false);
			base.OnExit();
		}

		public override float GetEnterStaminaCost()
		{
			return 1;
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			var result = !_attackConfig.LockedStateTime.Contains(NormalizedTime);
			if(result)
			{
				Debug.LogError(NormalizedTime);
			}
			return !_attackConfig.LockedStateTime.Contains(NormalizedTime);
		}

		private void ApplyGuaranteedDamage(HitData hitData)
		{
			if(_target == null || _target.ExternalData.IsDead)
			{
				return;
			}

			var hitConfig = hitData.Config;
			var damageAmount = _attackConfig.BaseDamage * hitConfig.DamageMultiplier;

			var damageInfo = new DamageInfo
			{
				DamageAmount = damageAmount,
				PoiseDamageAmount = hitConfig.PoiseDamage,
				WorldPos = _target.ExternalData.Transform.Position,
				DoneByPlayer = _context.IsPlayer.Value,
				DamageDealer = _context.SelfLink,
				DeflectionRating = 0
			};

			_target.ExternalData.ApplyDamage.Execute(damageInfo);
		}

	
	}
}
