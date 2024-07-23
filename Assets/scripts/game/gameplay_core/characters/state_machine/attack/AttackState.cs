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
			for(var i = 0; i < _currentAttackConfig.HitConfigs.Count; i++)
			{
				_hitsData.Add(new HitData()
				{
					Config = _currentAttackConfig.HitConfigs[i], 
				});
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

			foreach (var hitData in _hitsData)
			{
				var startEndTime = hitData.Config.Timing;

				if(!hitData.IsStarted && NormalizedTime >= startEndTime.x)
				{
					hitData.IsStarted = true;
				}

				if(hitData.IsActive)
				{
					//overlap capsule

					if(NormalizedTime >= startEndTime.y)
					{
						hitData.IsEnded = true;
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
			public HitConfig Config;

			public HashSet<string> TargetedCharacters = new();
		}
	}
}
