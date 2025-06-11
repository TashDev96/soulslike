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
		private const int FramesToUnlockWalkAfterStateUnlocked = 5;
		private int _currentAttackIndex;
		private AttackType _attackType;
		private AttackConfig _currentAttackConfig;
		private readonly List<HitData> _hitsData = new();
		private int _comboCounter;

		private int _framesToUnlockWalk;

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

			UpdateForwardMovement(_currentAttackConfig.ForwardMovement.Evaluate(Time), deltaTime);

			var hasActiveHit = false;

			foreach(var hitData in _hitsData)
			{
				var hitTiming = hitData.Config.Timing;

				if(!hitData.IsStarted && NormalizedTime >= hitTiming.x)
				{
					hitData.IsStarted = true;
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
			_currentAttackConfig = GetCurrentAttackConfig();
			Duration = _currentAttackConfig.Duration;

			_hitsData.Clear();
			for(var i = 0; i < _currentAttackConfig.HitConfigs.Count; i++)
			{
				_hitsData.Add(new HitData
				{
					Config = _currentAttackConfig.HitConfigs[i]
				});
			}

			var newAnimation = _context.Animator.Play(_currentAttackConfig.Animation, 0.1f, FadeMode.FromStart);

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
				Time = time * newAnimation.Duration;
				newAnimation.Time = time * newAnimation.Duration;
				ResetForwardMovement(_currentAttackConfig.ForwardMovement.Evaluate(Time));
			}
		}

		private AttackConfig GetCurrentAttackConfig()
		{
			var weaponConfig = _context.WeaponView.Value.Config;
			if(_context.InputData.ForcedAttackConfig != null)
			{
				return _context.InputData.ForcedAttackConfig;
			}

			switch(_attackType)
			{
				case AttackType.Regular:
				case AttackType.Strong:
					var attacksList = weaponConfig.GetAttacksSequence(_attackType);

					if(_comboCounter > 0)
					{
						_currentAttackIndex = _comboCounter % attacksList.Length;
					}
					else
					{
						_currentAttackIndex = 0;
					}

					return attacksList[_currentAttackIndex];
				case AttackType.RollAttackRegular:
					_currentAttackIndex = 0;
					return _context.WeaponView.Value.Config.RollAttack;
				case AttackType.RollAttackStrong:
					_currentAttackIndex = 0;
					return _context.WeaponView.Value.Config.RollAttackStrong;
				case AttackType.RunAttackRegular:
					_currentAttackIndex = 0;
					return _context.WeaponView.Value.Config.RunAttack;
				case AttackType.RunAttackStrong:
					_currentAttackIndex = 0;
					return _context.WeaponView.Value.Config.RunAttackStrong;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
