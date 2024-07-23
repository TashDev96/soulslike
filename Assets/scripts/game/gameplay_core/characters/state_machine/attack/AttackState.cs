using System.Collections.Generic;
using System.Linq;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public class AttackState : BaseCharacterState
	{
		private int _currentAttackIndex = 0;
		private int _lastAttackType = 0;
		private bool _comboTriggered;
		private float _time;
		private CharacterCommand _type;
		private AttackConfig _currentAttackConfig;
		private List<HitData> _hitsData = new();

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
			for(var i = 0; i < _currentAttackConfig.HitTimings.Count; i++)
			{
				_hitsData.Add(new HitData());
			}

			Debug.Log($"attack {_currentAttackIndex} {_currentAttackIndex % (attacksList.Length - 1)} {_type}");
			IsComplete = false;
			_time = 0;
		}

		public override void Update(float deltaTime)
		{
			_time += deltaTime;

			if(!_hitsData.Any(d => d.IsStarted))
			{
				RotateCharacter(_context.InputData.DirectionWorld, _context.RotationSpeed.Value, deltaTime);
			}

			for(var i = 0; i < _hitsData.Count; i++)
			{
				var startEndTime = _currentAttackConfig.HitTimings[i];

				if(!_hitsData[i].IsStarted && NormalizedTime >= startEndTime.x)
				{
					_hitsData[i].IsStarted = true;
				}

				if(_hitsData[i].IsActive)
				{
					//overlap capsule

					if(NormalizedTime >= startEndTime.y)
					{
						_hitsData[i].IsEnded = true;
					}
				}
			}

			if(_time >= _currentAttackConfig.Duration)
			{
				IsComplete = true;
			}

			IsReadyToRememberNextCommand = TimeLeft < 3f;
		}

		private float NormalizedTime => _time / _currentAttackConfig.Duration;
		private float TimeLeft => _currentAttackConfig.Duration - _time;

		public void TryContinueCombo()
		{
			_comboTriggered = true;
		}

		public void SetType(CharacterCommand nextCommand)
		{
			_type = nextCommand;
		}

		private class HitData
		{
			public bool IsStarted;
			public bool IsEnded;
			public bool IsActive => IsStarted && !IsEnded;
			public HashSet<string> TargetedCharacters = new();
		}
	}
}
