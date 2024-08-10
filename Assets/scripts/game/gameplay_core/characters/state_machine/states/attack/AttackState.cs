using System.Collections.Generic;
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
		private bool _comboTriggered;
		private float _time;
		private CharacterCommand _type;
		private AttackConfig _currentAttackConfig;
		private readonly List<HitData> _hitsData = new();

		private float NormalizedTime => _time / _currentAttackConfig.Duration;
		private float TimeLeft => _currentAttackConfig.Duration - _time;

		public AttackState(CharacterContext context) : base(context)
		{
		}

		public override void OnEnter()
		{
			if(_comboTriggered)
			{
				_currentAttackIndex++;
				_comboTriggered = false;
			}
			else
			{
				_currentAttackIndex = 0;
			}

			var weaponConfig = _context.CurrentWeapon.Value.Config;

			AttackConfig[] attacksList = null;

			switch(_type)
			{
				case CharacterCommand.Attack:
					attacksList = weaponConfig.RegularAttacks;
					break;
				case CharacterCommand.StrongAttack:
					attacksList = weaponConfig.StrongAttacks;
					break;
			}

			_currentAttackConfig = attacksList[_currentAttackIndex % (attacksList.Length - 1)];

			_hitsData.Clear();
			for(var i = 0; i < _currentAttackConfig.HitConfigs.Count; i++)
			{
				_hitsData.Add(new HitData
				{
					Config = _currentAttackConfig.HitConfigs[i]
				});
			}

			_context.Animator.Play(_currentAttackConfig.Animation);

			Debug.Log($"attack {_currentAttackIndex} {_currentAttackIndex % (attacksList.Length - 1)} {_type}");
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

		public void TryContinueCombo()
		{
			_comboTriggered = true;
		}

		public override bool CanExecuteNextCommand(CharacterCommand command)
		{
			switch(command)
			{
				case CharacterCommand.Walk:
				case CharacterCommand.Run:
				case CharacterCommand.Roll:
				case CharacterCommand.UseItem:
				case CharacterCommand.Interact:
					return !_currentAttackConfig.LockedStateTime.Contains(NormalizedTime);
				case CharacterCommand.Attack:
				case CharacterCommand.StrongAttack:
				case CharacterCommand.Block:
					return IsComplete;
			}
			return IsComplete;
		}

		public void SetType(CharacterCommand nextCommand)
		{
			_type = nextCommand;
		}
	}
}
