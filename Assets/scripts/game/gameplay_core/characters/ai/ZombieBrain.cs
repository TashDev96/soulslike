using System;
using dream_lib.src.utils.components;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.ai.navigation;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.characters.state_machine.states.attack;
using game.gameplay_core.damage_system;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	public class ZombieBrain : MonoBehaviour, ICharacterBrain
	{
		private const float DecisionPeriodSeconds = 1f;
		[SerializeField]
		private TriggerEventsListener[] _aggroZones;
		[SerializeField]
		private float _attackDistance = 2f;
		[SerializeField]
		private float _fightModeDistance = 4f;

		[Header("Stupidity")]
		[SerializeField]
		private RangeFloat _freezeInterval;
		[SerializeField]
		private RangeFloat _freezeDuration;

		[SerializeField]
		private RangeFloat _noAttackInterval;
		[SerializeField]
		private RangeFloat _noAttackDuration;
		[SerializeField] [Range(0, 1)]
		private float _pauseBetweenAttacksChance;
		[SerializeField]
		private RangeFloat _pauseBetweenAttacksDuration;

		[SerializeField] [Range(0, 1)]
		private float _mistakeAttackChance;
		[SerializeField] [Range(0, 1)]
		private float _dodgeTriggerChance;

		[Header("Debug")]
		[SerializeField]
		private Color _navigationDebugColor = Color.green;
		[SerializeField]
		private bool _drawNavigationDebug;

		private State _state;
		private CharacterDomain _target;
		private CharacterContext _context;
		private AiNavigationModule _navigationModule;

		private float _decisionTimer;

		private FightStateData _fightData = new();

		private bool HasTarget => _target != null;

		public void Initialize(CharacterContext context)
		{
			_context = context;
			_context.ApplyDamage.OnExecute += HandleDamage;

			foreach(var triggerListener in _aggroZones)
			{
				triggerListener.OnTriggerEnterEvent += HandleAggroTriggerEnter;
			}

			_context.CurrentState.OnChangedFromTo += HandleCharacterStateChange;

			_navigationModule = new AiNavigationModule(_context.Transform);
		}

		public void Think(float deltaTime)
		{
			var isReadyToMakeDecision = false;

			_decisionTimer -= deltaTime;
			if(_decisionTimer <= 0)
			{
				_decisionTimer = DecisionPeriodSeconds;
				isReadyToMakeDecision = true;
			}

			var selfPosition = _context.Transform.Position;
			Vector3 directionToTarget = default;
			float distanceToTarget = default;
			if(HasTarget)
			{
				var targetPosition = _target.ExternalData.Transform.Position;
				var vectorToTarget = targetPosition - selfPosition;
				directionToTarget = vectorToTarget.normalized;
				distanceToTarget = vectorToTarget.magnitude;
			}

			switch(_state)
			{
				case State.Idle:

					if(HasTarget)
					{
						BuildPathToTarget();
						_state = State.Navigate;
					}

					break;
				case State.Navigate:

					if(!HasTarget)
					{
						_state = State.Idle;
						break;
					}

					if(distanceToTarget < _fightModeDistance)
					{
						_state = State.Fight;
						break;
					}

					if(isReadyToMakeDecision)
					{
						BuildPathToTarget();
					}

					if(!_navigationModule.Path.IsEmpty)
					{
						_context.InputData.Command = CharacterCommand.Walk;

						if(_navigationModule.Path.IsEmpty)
						{
							_context.InputData.DirectionWorld = directionToTarget;
						}
						else
						{
							var newDir = _navigationModule.CalculateMoveDirection(selfPosition);
							_context.InputData.DirectionWorld = newDir;
						}
					}

					break;
				case State.Fight:

					UpdateFightState(isReadyToMakeDecision, distanceToTarget, directionToTarget);

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public string GetDebugSting()
		{
			return $"Zombie Brain, state {_state}";
		}

		private void HandleDamage(DamageInfo info)
		{
			if(!HasTarget)
			{
				_target = info.DamageDealer;
			}
		}

		private void UpdateFightState(bool isReadyToMakeDecision, float distanceToTarget, Vector3 directionToTarget)
		{
			if(!HasTarget)
			{
				_state = State.Idle;
				return;
			}

			if(isReadyToMakeDecision && distanceToTarget > _fightModeDistance)
			{
				BuildPathToTarget();
				_state = State.Navigate;
				return;
			}

			if(distanceToTarget < _attackDistance)
			{
				_context.InputData.Command = CharacterCommand.Attack;
				_context.InputData.DirectionWorld = directionToTarget;
			}
			else
			{
				_context.InputData.Command = CharacterCommand.Walk;
				_context.InputData.DirectionWorld = directionToTarget;
			}
		}

		private void HandleCharacterStateChange(CharacterStateBase from, CharacterStateBase to)
		{
			if(from is AttackState && to is not AttackState)
			{
				// _context.CurrentWeapon.Value.Config.RegularAttacks
			}
		}

		private void BuildPathToTarget()
		{
			_navigationModule.BuildPath(_target.ExternalData.Transform.Position);
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if(!Application.isPlaying)
			{
				return;
			}
			if(_drawNavigationDebug)
			{
				if(_navigationModule != null && !_navigationModule.Path.IsEmpty)
				{
					for(var i = 1; i < _navigationModule.Path.Positions.Count; i++)
					{
						var prevPos = _navigationModule.Path.Positions[i - 1];
						var pos = _navigationModule.Path.Positions[i];
						//Debug.DrawLine(prevPos, pos, _navigationDebugColor);
					}
				}
			}
		}
#endif

		private void HandleAggroTriggerEnter(Collider enteredObject)
		{
			if(HasTarget)
			{
				return;
			}

			if(enteredObject.gameObject.TryGetComponent<CharacterDomain>(out var otherCharacter))
			{
				if(otherCharacter.ExternalData.Team != _context.Team.Value)
				{
					_target = otherCharacter;
				}
			}
		}

		private class FightStateData
		{
			public float Duration;
			public float TotalDurationWithoutTargetLoss;
			public float NextEvadeChance;

			public float TimeToNextStupidity;
		}

		private enum State
		{
			Idle,
			Navigate,
			Fight
		}
	}
}
