using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states.attack
{
	public class RiposteState : CharacterAnimationStateBase
	{
		private const string StaminaRegenDisableKey = "RiposteState";

		private bool _staminaSpent;
		private bool _staminaRegenDisabled;
		private readonly List<HitData> _hitsData = new();
		private readonly CharacterDomain _riposteTarget;

		public AnimancerState CurrentRiposteAnimation { get; private set; }
		public AttackConfig RiposteAttackConfig { get; private set; }

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public RiposteState(CharacterContext context, CharacterDomain riposteTarget) : base(context)
		{
			_riposteTarget = riposteTarget;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			RiposteAttackConfig = _context.RightWeapon.Value.Config.RiposteAttack;
			Duration = RiposteAttackConfig.Duration;

			_staminaSpent = false;
			_hitsData.Clear();
			for(var i = 0; i < RiposteAttackConfig.HitConfigs.Count; i++)
			{
				_hitsData.Add(new HitData
				{
					Config = RiposteAttackConfig.HitConfigs[i]
				});
			}

			CurrentRiposteAnimation = _context.Animator.Play(RiposteAttackConfig.Animation, 0.1f, FadeMode.FromStart);

			Time = 0f;
			ResetForwardMovement();

			LockTargetInAnimation();
		}

		public override void Update(float deltaTime)
		{
			Time += deltaTime;

			if(_context.InputData.HasDirectionInput && !RiposteAttackConfig.RotationDisabledTime.Contains(NormalizedTime))
			{
				_context.MovementLogic.RotateCharacter(_context.InputData.DirectionWorld, deltaTime);
			}

			UpdateStaminaRegenLock();

			UpdateForwardMovement(RiposteAttackConfig.ForwardMovement.Evaluate(Time), deltaTime);

			foreach(var hitData in _hitsData)
			{
				var hitTiming = hitData.Config.Timing;

				if(!hitData.IsStarted && NormalizedTime >= hitTiming.x)
				{
					hitData.IsStarted = true;
					if(!_staminaSpent)
					{
						_context.StaminaLogic.SpendStamina(RiposteAttackConfig.StaminaCost);
						_staminaSpent = true;
					}
					ApplyGuaranteedDamage(hitData);
				}

				if(hitData.IsStarted && NormalizedTime >= hitTiming.y)
				{
					hitData.IsEnded = true;
				}
			}

			if(Time >= RiposteAttackConfig.Duration)
			{
				IsComplete = true;
			}

			IsReadyToRememberNextCommand = TimeLeft < 3f;

			void UpdateStaminaRegenLock()
			{
				if(!_staminaRegenDisabled)
				{
					if(RiposteAttackConfig.StaminaRegenDisabledTime.Contains(NormalizedTime))
					{
						_staminaRegenDisabled = true;
						_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, true);
					}
				}
				else if(!RiposteAttackConfig.StaminaRegenDisabledTime.Contains(NormalizedTime))
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
			var weaponConfig = _context.RightWeapon.Value.Config;
			return weaponConfig.RiposteAttack.StaminaCost;
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			var result = !RiposteAttackConfig.LockedStateTime.Contains(NormalizedTime);
			if(result)
			{
				Debug.LogError(NormalizedTime);
			}
			return !RiposteAttackConfig.LockedStateTime.Contains(NormalizedTime);
		}

		private void ApplyGuaranteedDamage(HitData hitData)
		{
			if(_riposteTarget == null || _riposteTarget.ExternalData.IsDead)
			{
				return;
			}

			var hitConfig = hitData.Config;
			var damageAmount = RiposteAttackConfig.BaseDamage * hitConfig.DamageMultiplier;

			var damageInfo = new DamageInfo
			{
				DamageAmount = damageAmount,
				PoiseDamageAmount = hitConfig.PoiseDamage,
				WorldPos = _riposteTarget.ExternalData.Transform.Position,
				DoneByPlayer = _context.IsPlayer.Value,
				DamageDealer = _context.SelfLink,
				DeflectionRating = 0
			};

			_riposteTarget.ExternalData.ApplyDamage.Execute(damageInfo);
		}

		private void LockTargetInAnimation()
		{
			if(_riposteTarget == null || _riposteTarget.ExternalData.IsDead)
			{
				return;
			}
			
			_riposteTarget.transform.rotation = Quaternion.LookRotation( (_context.Transform.Position-_riposteTarget.transform.position).SetY(0) );
			_riposteTarget.transform.position = _context.Transform.Position - _riposteTarget.transform.forward * 1.5f;

			var targetAnimationClip = _riposteTarget.ExternalData.Config.RipostedAnimation;
			if(targetAnimationClip != null)
			{
				_riposteTarget.CharacterStateMachine.LockInAnimation(targetAnimationClip);
			}
		}

	 
	}
}
