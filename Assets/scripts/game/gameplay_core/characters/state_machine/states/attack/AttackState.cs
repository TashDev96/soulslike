using System.Collections.Generic;
using Animancer;
using dream_lib.src.extensions;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.runtime_data;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states.attack
{
	public class AttackState : CharacterStateBase
	{
		private int _currentAttackIndex;
		private int _lastAttackType = 0;
		public float Time { get; private set; }
		private AttackType _attackType;
		private AttackConfig _currentAttackConfig;
		private readonly List<HitData> _hitsData = new();
		private int _comboCounter;

		private float NormalizedTime => Time / _currentAttackConfig.Duration;
		private float TimeLeft => _currentAttackConfig.Duration - Time;

		private int _framesToUnlockWalk = 0;

		public AttackState(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			_comboCounter = 0;
			LaunchAttack();
		}

		private void LaunchAttack()
		{
			var weaponConfig = _context.CurrentWeapon.Value.Config;
			var attacksList = weaponConfig.GetAttacksSequence(_attackType);

			if(_comboCounter > 0)
			{
				_currentAttackIndex = _comboCounter % (attacksList.Length);
			}
			else
			{
				_currentAttackIndex = 0;
			}

			_currentAttackConfig = attacksList[_currentAttackIndex];

			_hitsData.Clear();
			for(var i = 0; i < _currentAttackConfig.HitConfigs.Count; i++)
			{
				_hitsData.Add(new HitData
				{
					Config = _currentAttackConfig.HitConfigs[i]
				});
			}

			var newAnimation = _context.Animator.Play(_currentAttackConfig.Animation, 0.1f, FadeMode.FromStart);
			if(_comboCounter > 0)
			{
				newAnimation.Time = _currentAttackConfig.EnterComboTime;
			}

			_context.DebugDrawer.Value.AddAttackGraph(_currentAttackConfig);

			Debug.Log($"attack {_comboCounter} {_currentAttackIndex} {_attackType}");
			IsComplete = false;
			Time = 0;
		}

		public override void Update(float deltaTime)
		{
			Time += deltaTime;

			if(_context.InputData.HasDirectionInput && !_currentAttackConfig.RotationDisabledTime.Contains(NormalizedTime))
			{
				RotateCharacter(_context.InputData.DirectionWorld, _context.RotationSpeed.Value.DegreesPerSecond, deltaTime);
			}

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
					_context.CurrentWeapon.Value.CastAttack(_currentAttackConfig, hitData);

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
				_framesToUnlockWalk = 5;
			}

			IsReadyToRememberNextCommand = TimeLeft < 3f;
		}

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			if(nextCommand is not CharacterCommand.Attack and not CharacterCommand.StrongAttack)
			{
				return false;
			}

			_context.DebugDrawer.Value.AddAttackComboAttempt(Time);

			if(_currentAttackConfig.ExitToComboTime.Contains(NormalizedTime))
			{
				_comboCounter++;
				LaunchAttack();
				return true;
			}
			return false;
		}

		public override bool CheckIsReadyToChangeState(CharacterCommand nextCommand)
		{
			if(nextCommand == CharacterCommand.Walk)
			{
				if(_framesToUnlockWalk > 0)
				{
					_framesToUnlockWalk--;
					return false;
				}
			}
			
			return !_currentAttackConfig.LockedStateTime.Contains(NormalizedTime);
		}
	}
}
