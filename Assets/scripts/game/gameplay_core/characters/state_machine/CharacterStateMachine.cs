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
		private readonly FallState _fallState;
		private readonly RunState _runState;
		private readonly StayBlockState _stayBlockState;
		private readonly WalkBlockState _walkBlockState;

		private CharacterCommand _nextCommand;
		private readonly ReactiveProperty<CharacterStateBase> _currentState = new();

		private CharacterCommand NextCommand
		{
			get => _nextCommand;
			set
			{
				if(value != _nextCommand)
				{
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
			_fallState = new FallState(_context);
			_runState = new RunState(_context);
			_stayBlockState = new StayBlockState(_context);
			_walkBlockState = new WalkBlockState(_context);

			_context.IsDead.OnChanged += HandleIsDeadChanged;
			_context.TriggerStagger.OnExecute += HandleTriggerStagger;
			_context.DeflectCurrentAttack.OnExecute += HnadleAttackDeflected;

			_context.IsFalling.OnChangedFromTo += HandleIsFallingChanged;

			SetState(_idleState);
		}

		private void HnadleAttackDeflected()
		{
			if(_currentState.Value is AttackState attackState)
			{
				SetState(new DeflectedAttackState(_context, attackState.CurrentAttackAnimation, attackState.CurrentAttackConfig));
			}
		}

		public void Update(float deltaTime, bool calculateInputLogic)
		{
			_currentState.Value.Update(deltaTime);

			calculateInputLogic &= !_context.IsDead.Value;

			if(calculateInputLogic)
			{
				TryRememberNextCommand();
				CalculateChangeState();
			}
		}

		public string GetDebugString()
		{
			var str = "";
			str += $"state:   {_currentState.Value.GetType().Name}  complete: {_currentState.Value.IsComplete}\n";
			str += $"{_currentState.Value.GetDebugString()}\n";
			str += $"command: {_context.InputData.Command}\n";
			str += $"next command: {NextCommand}\n";
			return str;
		}

		private void HandleIsFallingChanged(bool wasFalling, bool isFalling)
		{
			if(isFalling && !(_currentState.Value is FallState) && !_context.IsDead.Value)
			{
				if(!(_currentState.Value is AttackState) && !(_currentState.Value is RollState) && !(_currentState.Value is StaggerState))
				{
					_currentState.Value.OnInterrupt();
					SetState(_fallState);
				}
			}

			if(!isFalling && wasFalling && _currentState.Value is FallState fallState)
			{
				if(fallState.ShouldRollOnLanding)
				{
					SetState(_rollState);
				}
			}
		}

		private void HandleTriggerStagger()
		{
			if(_currentState.Value.CanInterruptByStagger && !_context.IsDead.Value)
			{
				Debug.Log("stag");
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

		private void CalculateChangeState()
		{
			if(_context.IsFalling.Value && _currentState.Value is not FallState)
			{
				if(_currentState.Value.IsComplete || _currentState.Value.CheckIsReadyToChangeState(CharacterCommand.Walk))
				{
					SetState(_fallState);
				}
			}

			if(_currentState.Value is RunState && _context.CharacterStats.Stamina.Value <= 0)
			{
				if(NextCommand == CharacterCommand.Run)
				{
					SetState(_walkState);
				}
			}

			if(_currentState.Value.TryContinueWithCommand(NextCommand))
			{
				NextCommand = CharacterCommand.None;
				_context.InputData.Command = CharacterCommand.None;
				return;
			}

			if(NextCommand.IsAttackCommand() && _currentState.Value is RollState { CanSwitchToAttack: true })
			{
				var rollAttackType = NextCommand is CharacterCommand.StrongAttack ? AttackType.RollAttackStrong : AttackType.RollAttackRegular;
				_attackState.SetEnterParams(rollAttackType);
				SetState(_attackState);
				NextCommand = CharacterCommand.None;
				return;
			}

			if(_currentState.Value == _fallState && _fallState.IsComplete && _fallState.HasValidRollInput)
			{
				SetState(_rollState);
			}

			if(_currentState.Value.CheckIsReadyToChangeState(NextCommand))
			{
				switch(NextCommand)
				{
					case CharacterCommand.None:
						if(_currentState.Value.IsComplete && _currentState.Value != _idleState)
						{
							SetState(_idleState);
						}
						break;
					case CharacterCommand.Walk:
						SetState(_walkState);
						break;
					case CharacterCommand.Run:
						SetState(_runState);
						break;
					case CharacterCommand.Roll:
						SetState(_rollState);
						break;
					case CharacterCommand.RegularAttack:
						_attackState.SetEnterParams(_currentState.Value is RunState ? AttackType.RunAttackRegular : AttackType.Regular);
						SetState(_attackState);
						break;
					case CharacterCommand.StrongAttack:
						_attackState.SetEnterParams(_currentState.Value is RunState ? AttackType.RunAttackStrong : AttackType.Strong);
						SetState(_attackState);
						break;
					case CharacterCommand.StayBlock:
						SetState(_stayBlockState);
						break;
					case CharacterCommand.WalkBlock:
						SetState(_walkBlockState);
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

			if(!_context.StaminaLogic.CheckCanEnterState(newState))
			{
				if(newState is RunState)
				{
					if(_currentState.Value is WalkState)
					{
						return;
					}
					newState = _walkState;
				}
				else
				{
					newState = _idleState;
				}
			}
			
			_currentState.Value = newState;
			_currentState.Value.OnEnter();
			_context.OnStateChanged.Execute(oldState, newState);
			NextCommand = CharacterCommand.None;
		}
	}
}
