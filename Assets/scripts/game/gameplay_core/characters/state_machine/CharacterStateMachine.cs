using System;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.state_machine.states;
using game.gameplay_core.characters.state_machine.states.attack;

namespace game.gameplay_core.characters.state_machine
{
	public class CharacterStateMachine
	{
		private readonly CharacterContext _context;

		private readonly IdleState _idleState;
		private readonly WalkState _walkState;
		private readonly AttackState _attackState;
		private readonly StaggerState _staggerState;
		private CharacterCommand _nextCommand;

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

		public CharacterStateBase CurrentState { get; private set; }

		public CharacterStateMachine(CharacterContext characterContext)
		{
			_context = characterContext;

			_idleState = new IdleState(_context);
			_walkState = new WalkState(_context);
			_attackState = new AttackState(_context);
			_staggerState = new StaggerState(_context);

			_context.IsDead.OnChanged += HandleIsDeadChanged;
			_context.TriggerStagger.OnExecute += HandleTriggerStagger;

			SetState(_idleState);
		}

		public void Update(float deltaTime, bool calculateInputLogic)
		{
			CurrentState.Update(deltaTime);

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
			str += $"state:   {CurrentState.GetType().Name}  complete: {CurrentState.IsComplete}\n";
			str += $"command: {_context.InputData.Command}\n";
			str += $"next command: {NextCommand}\n";
			return str;
		}

		private void HandleTriggerStagger()
		{
			if(CurrentState.CanInterruptByStagger && !_context.IsDead.Value)
			{
				CurrentState.OnInterrupt();
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

			if(NextCommand == CharacterCommand.None)
			{
				if(CurrentState.IsReadyToRememberNextCommand)
				{
					NextCommand = inputCommand;
				}
			}
		}

		private void TryExecuteNextCommand()
		{
			if(CurrentState.TryContinueWithCommand(NextCommand))
			{
				NextCommand = CharacterCommand.None;
				return;
			}

			if(CurrentState.CheckIsReadyToChangeState())
			{
				switch(NextCommand)
				{
					case CharacterCommand.None:
						SetState(_idleState);
						break;
					case CharacterCommand.Walk:
						SetState(_walkState);
						break;
					case CharacterCommand.Run:
						break;
					case CharacterCommand.Roll:
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

		private bool CheckIsContinuousCommand(CharacterCommand command)
		{
			switch(command)
			{
				case CharacterCommand.None:
				case CharacterCommand.Walk:
				case CharacterCommand.Run:
				case CharacterCommand.Block:
					return true;
				default:
					return false;
			}
		}

		private void SetState(CharacterStateBase newState)
		{
			CurrentState?.OnExit();
			CurrentState = newState;
			CurrentState.OnEnter();
			NextCommand = CharacterCommand.None;
		}
	}
}
