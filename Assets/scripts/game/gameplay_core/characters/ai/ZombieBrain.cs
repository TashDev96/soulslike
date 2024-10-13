using System;
using dream_lib.src.utils.components;
using game.gameplay_core.characters.commands;
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

		private bool HasTarget => _target != null;

		public void Initialize(CharacterContext context)
		{
			_context = context;

			foreach(var triggerListener in _aggroZones)
			{
				triggerListener.OnTriggerEnterEvent += HandleAggroTriggerEnter;
			}

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
			Vector3 targetPosition = default;
			Vector3 vectorToTarget = default;
			Vector3 directionToTarget = default;
			float distanceToTarget = default;
			if(HasTarget)
			{
				targetPosition = _target.ExternalData.Transform.Position;
				vectorToTarget = targetPosition - selfPosition;
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
							var newDir = _navigationModule.CalculateMoveDirection(selfPosition, _context.WalkSpeed.Value);
							_context.InputData.DirectionWorld = newDir;
						}
					}

					break;
				case State.Fight:

					if(!HasTarget)
					{
						_state = State.Idle;
						break;
					}

					if(isReadyToMakeDecision && distanceToTarget > _fightModeDistance)
					{
						BuildPathToTarget();
						_state = State.Navigate;
						break;
					}

					if(distanceToTarget < _attackDistance)
					{
						_context.InputData.Command = CharacterCommand.Attack;
						_context.InputData.DirectionWorld = directionToTarget;
					}
					else
					{
						_context.InputData.DirectionWorld = directionToTarget;
					}

					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		
		
		private void BuildPathToTarget()
		{
			_navigationModule.BuildPath(_target.ExternalData.Transform.Position);
		}

		public string GetDebugSting()
		{
			return $"Zombie Brain, state {_state}";
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
						Debug.DrawLine(prevPos, pos, _navigationDebugColor);
					}

					var refPos = _navigationModule.ReferencePos;
					Debug.DrawLine(refPos, refPos+Vector3.up*3f, _navigationDebugColor);
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

		private enum State
		{
			Idle,
			Navigate,
			Fight
		}
	}
}
