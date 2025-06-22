using System;
using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.state_machine.states.attack
{
	public class AttackState : CharacterAnimationStateBase
	{
		private const string StaminaRegenDisableKey = "AttackState";

		private const int FramesToUnlockWalkAfterStateUnlocked = 5;
		private int _currentAttackIndex;
		private AttackType _attackType;
		private AttackConfig _currentAttackConfig;
		private readonly List<HitData> _hitsData = new();
		private int _comboCounter;
		private bool _staminaSpent;
		private bool _staminaRegenDisabled;

		private int _framesToUnlockWalk;
		public AnimancerState CurrentAttackAnimation { get; private set; }
		public AttackConfig CurrentAttackConfig => _currentAttackConfig;

		public override float Time { get; protected set; }
		protected override float Duration { get; set; }

		public AttackState(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			_comboCounter = 0;
			LaunchAttack();
		}

		public override void Update(float deltaTime)
		{
			Time += deltaTime;

			if(_context.InputData.HasDirectionInput && !_currentAttackConfig.RotationDisabledTime.Contains(NormalizedTime))
			{
				_context.MovementLogic.RotateCharacter(_context.InputData.DirectionWorld, deltaTime);
			}
			
			UpdateStaminaRegenLock();

			UpdateForwardMovement(_currentAttackConfig.ForwardMovement.Evaluate(Time), deltaTime);

			var hasActiveHit = false;

			foreach(var hitData in _hitsData)
			{
				var hitTiming = hitData.Config.Timing;

				if(!hitData.IsStarted && NormalizedTime >= hitTiming.x)
				{
					hitData.IsStarted = true;
					if(!_staminaSpent)
					{
						_context.StaminaLogic.SpendStamina(_currentAttackConfig.StaminaCost);
					}
				}

				if(hitData.IsActive)
				{
					_context.WeaponView.Value.CastAttackInterpolated(_currentAttackConfig, hitData);

					if(NormalizedTime >= hitTiming.y)
					{
						hitData.IsEnded = true;
					}
				}

				hasActiveHit |= hitData.IsActive;
			}

			_context.MaxDeltaTime.Value = hasActiveHit ? CharacterConstants.MaxDeltaTimeAttacking : CharacterConstants.MaxDeltaTimeNormal;

			if(Time >= _currentAttackConfig.Duration)
			{
				IsComplete = true;
			}

			if(_currentAttackConfig.LockedStateTime.Contains(NormalizedTime))
			{
				_framesToUnlockWalk = FramesToUnlockWalkAfterStateUnlocked;
			}

			IsReadyToRememberNextCommand = TimeLeft < 3f;

			void UpdateStaminaRegenLock()
			{
				if(!_staminaRegenDisabled)
				{
					if(_currentAttackConfig.StaminaRegenDisabledTime.Contains(NormalizedTime))
					{
						_staminaRegenDisabled = true;
						_context.StaminaLogic.SetStaminaRegenLock(StaminaRegenDisableKey, true);
					}
				}
				else if(!_currentAttackConfig.StaminaRegenDisabledTime.Contains(NormalizedTime))
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

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			if(nextCommand is not CharacterCommand.RegularAttack and not CharacterCommand.StrongAttack)
			{
				return false;
			}

			_context.DebugDrawer.Value.AddAttackComboAttempt(Time);

			if(_currentAttackConfig.ExitToComboTime.Contains(NormalizedTime))
			{
				_comboCounter++;
				SetEnterParams(nextCommand is CharacterCommand.StrongAttack ? AttackType.Strong : AttackType.Regular);
				LaunchAttack();
				return true;
			}
			return false;
		}

		public override float GetEnterStaminaCost()
		{
			GetCurrentAttackConfig(out var config, out _);
			return config.StaminaCost;
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			if(nextCommand.IsMovementCommand())
			{
				if(_framesToUnlockWalk > 0)
				{
					_framesToUnlockWalk--;
					return false;
				}
			}

			return !_currentAttackConfig.LockedStateTime.Contains(NormalizedTime);
		}

		public void SetEnterParams(AttackType attackType)
		{
			_attackType = attackType;
		}

		private void LaunchAttack()
		{
			GetCurrentAttackConfig(out _currentAttackConfig, out _currentAttackIndex);
			Duration = _currentAttackConfig.Duration;

			_staminaSpent = false;
			_hitsData.Clear();
			for(var i = 0; i < _currentAttackConfig.HitConfigs.Count; i++)
			{
				_hitsData.Add(new HitData
				{
					Config = _currentAttackConfig.HitConfigs[i]
				});
			}

			CurrentAttackAnimation = _context.Animator.Play(_currentAttackConfig.Animation, 0.1f, FadeMode.FromStart);
			
			if(_attackType.IsRollAttack())
			{
				SetAttackInitialTime(_currentAttackConfig.EnterFromRollTime);
			}
			else
			{
				if(_comboCounter > 0)
				{
					SetAttackInitialTime(_currentAttackConfig.EnterComboTime);
				}
				else
				{
					Time = 0f;
					ResetForwardMovement();
				}
			}

			_context.DebugDrawer.Value.AddAttackGraph(_currentAttackConfig);

			IsComplete = false;

			void SetAttackInitialTime(float time)
			{
				Time = time * CurrentAttackAnimation.Duration;
				CurrentAttackAnimation.Time = time * CurrentAttackAnimation.Duration;
				ResetForwardMovement(_currentAttackConfig.ForwardMovement.Evaluate(Time));
			}
		}

		private void GetCurrentAttackConfig(out AttackConfig attackConfig, out int newAttackIndex)
		{
			var weaponConfig = _context.WeaponView.Value.Config;
			if(_context.InputData.ForcedAttackConfig != null)
			{
				newAttackIndex = _currentAttackIndex;
				attackConfig = _context.InputData.ForcedAttackConfig;
			}

			switch(_attackType)
			{
				case AttackType.Regular:
				case AttackType.Strong:
					var attacksList = weaponConfig.GetAttacksSequence(_attackType);

					if(_comboCounter > 0)
					{
						newAttackIndex = _comboCounter % attacksList.Length;
					}
					else
					{
						newAttackIndex = 0;
					}

					attackConfig = attacksList[_currentAttackIndex];
					return;
				case AttackType.RollAttackRegular:
					newAttackIndex = 0;
					attackConfig = _context.WeaponView.Value.Config.RollAttack;
					return;
				case AttackType.RollAttackStrong:
					newAttackIndex = 0;
					attackConfig = _context.WeaponView.Value.Config.RollAttackStrong;
					return;
				case AttackType.RunAttackRegular:
					newAttackIndex = 0;
					attackConfig = _context.WeaponView.Value.Config.RunAttack;
					return;
				case AttackType.RunAttackStrong:
					newAttackIndex = 0;
					attackConfig = _context.WeaponView.Value.Config.RunAttackStrong;
					return;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
