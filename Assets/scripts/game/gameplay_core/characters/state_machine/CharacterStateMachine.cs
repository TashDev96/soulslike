using System;
using dream_lib.src.reactive;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.characters.state_machine.states.attack;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public class CharacterStateMachine
	{
		private readonly CharacterContext _context;

		private readonly IdleState _idleState;
		private readonly WalkState _walkState;
		private readonly RollState _rollState;
		private readonly AttackState _attackState;
		private readonly StaggerState _staggerState;
		private CharacterCommand _nextCommand;
		private ReactiveProperty<CharacterStateBase> _currentState = new();

		private CharacterCommand NextCommand
		{
			get => _nextCommand;
			set
			{
				if(value != _nextCommand)
				{
					// Debug.Log($"{Time.frameCount}set command {value}");
				}
				_nextCommand = value;
			}
		}

		public IReadOnlyReactiveProperty<CharacterStateBase> CurrentState => _currentState;

		public CharacterStateMachine(CharacterContext characterContext)
		{
			_context = characterContext;

			_idleState = new IdleState(_context);
			_walkState = new WalkState(_context);
			_attackState = new AttackState(_context);
			_staggerState = new StaggerState(_context);
			_rollState = new RollState(_context);

			_context.IsDead.OnChanged += HandleIsDeadChanged;
			_context.TriggerStagger.OnExecute += HandleTriggerStagger;

			SetState(_idleState);
		}

		public void Update(float deltaTime, bool calculateInputLogic)
		{
			_currentState.Value.Update(deltaTime);

			calculateInputLogic &= !_context.IsDead.Value;

			if(calculateInputLogic)
			{
				TryRememberNextCommand();
				TryExecuteNextCommand();
			}
		}

		public string GetDebugString()
		{
			var str = "";
			str += $"state:   {_currentState.Value.GetType().Name}  complete: {_currentState.Value.IsComplete}\n";
			str += $"command: {_context.InputData.Command}\n";
			str += $"next command: {NextCommand}\n";
			return str;
		}

		private void HandleTriggerStagger()
		{
			if(_currentState.Value.CanInterruptByStagger && !_context.IsDead.Value)
			{
				_currentState.Value.OnInterrupt();
				SetState(_staggerState);
			}
		}

		private void HandleIsDeadChanged(bool isDead)
		{
			if(isDead)
			{
				SetState(new DeadState(_context));
			}
		}

		private void TryRememberNextCommand()
		{
			var inputCommand = _context.InputData.Command;

			var overrideMovement = NextCommand.IsMovementCommand() && !inputCommand.IsMovementCommand() && inputCommand != CharacterCommand.None;

			if(NextCommand == CharacterCommand.None || overrideMovement)
			{
				if(_currentState.Value.IsReadyToRememberNextCommand)
				{
					NextCommand = inputCommand;
				}
			}
		}

		private void TryExecuteNextCommand()
		{
			if(_currentState.Value.TryContinueWithCommand(NextCommand))
			{
				NextCommand = CharacterCommand.None;
				_context.InputData.Command = CharacterCommand.None;
				return;
			}

			if(NextCommand == CharacterCommand.Attack && _currentState.Value is RollState rollState)
			{
				if(rollState.CheckIsReadyToChangeState(NextCommand))
				{
					_attackState.IsRollAttackTriggered = true;
					SetState(_attackState);
					NextCommand = CharacterCommand.None;
					return;
				}
			}

			if(_currentState.Value.CheckIsReadyToChangeState(NextCommand))
			{
				switch(NextCommand)
				{
					case CharacterCommand.None:
						if(_currentState.Value.IsComplete)
						{
							SetState(_idleState);
						}
						break;
					case CharacterCommand.Walk:
						SetState(_walkState);
						break;
					case CharacterCommand.Run:
						SetState(_walkState);
						break;
					case CharacterCommand.Roll:
						SetState(_rollState);
						break;
					case CharacterCommand.Attack:
					case CharacterCommand.StrongAttack:

						SetState(_attackState);

						break;
					case CharacterCommand.Block:
						break;
					case CharacterCommand.UseItem:
						break;
					case CharacterCommand.Interact:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				NextCommand = CharacterCommand.None;
			}
		}

		private void SetState(CharacterStateBase newState)
		{
			_currentState.Value?.OnExit();
			var oldState = _currentState.Value;
			_currentState.Value = newState;
			_currentState.Value.OnEnter();
			_context.OnStateChanged.Execute(oldState, newState);
			NextCommand = CharacterCommand.None;
		}
	}
}
