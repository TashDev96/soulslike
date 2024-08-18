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
		private float _time;
		private AttackType _attackType;
		private AttackConfig _currentAttackConfig;
		private readonly List<HitData> _hitsData = new();
		private int _comboCounter;

		private float NormalizedTime => _time / _currentAttackConfig.Duration;
		private float TimeLeft => _currentAttackConfig.Duration - _time;

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

			var newAnimation = _context.Animator.Play(_currentAttackConfig.Animation,0.1f, FadeMode.FromStart);
			//newAnimation.Time = 0.1f //TODO: skip time for smooth transition

			Debug.Log($"attack {_comboCounter} {_currentAttackIndex} {_attackType}");
			IsComplete = false;
			_time = 0;
		}

		public override void Update(float deltaTime)
		{
			_time += deltaTime;

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

			if(_time >= _currentAttackConfig.Duration)
			{
				IsComplete = true;
			}

			IsReadyToRememberNextCommand = TimeLeft < 3f;
		}

		public override bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			if(nextCommand is not CharacterCommand.Attack and not CharacterCommand.StrongAttack)
			{
				return false;
			}
			
			if( _currentAttackConfig.EnterComboTime.Contains(NormalizedTime))
			{
				_comboCounter++;
				LaunchAttack();
				return true;
			}
			return false;
		}

		public override bool CheckIsReadyToChangeState()
		{
			return !_currentAttackConfig.LockedStateTime.Contains(NormalizedTime);
		}
	}
}
