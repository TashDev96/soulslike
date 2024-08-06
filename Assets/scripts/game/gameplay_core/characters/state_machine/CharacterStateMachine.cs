using System;

namespace game.gameplay_core.characters.state_machine
{
	public class CharacterStateMachine
	{
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

		private readonly CharacterContext _context;
		public BaseCharacterState CurrentState { get; private set; }

		private IdleState _idleState;
		private WalkState _walkState;
		private AttackState _attackState;
		private CharacterCommand _nextCommand;

		public CharacterStateMachine(CharacterContext characterContext)
		{
			_context = characterContext;

			_idleState = new IdleState(_context);
			_walkState = new WalkState(_context);
			_attackState = new AttackState(_context);

			_context.IsDead.OnChanged += HandleIsDeadChanged;
			
			SetState(_idleState);
		}

		private void HandleIsDeadChanged(bool isDead)
		{
			if(isDead)
			{
				SetState(new DeadState(_context));
			}
		}

		public void Update(float deltaTime, bool calculateInputLogic)
		{
			calculateInputLogic &= !_context.IsDead.Value;

			if(calculateInputLogic)
			{
				TryRememberNextCommand();
			}

			CurrentState.Update(deltaTime);

			if(calculateInputLogic)
			{
				TryChangeState();
			}
		}

		private void TryRememberNextCommand()
		{
			var inputCommand = _context.InputData.Command;

			if(NextCommand == CharacterCommand.None)
			{
				if(CheckIsContinuousCommand(inputCommand))
				{
					return;
				}

				if(CurrentState.IsReadyToRememberNextCommand)
				{
					NextCommand = inputCommand;
				}
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

		private void TryChangeState()
		{
			var commandToCalculate = NextCommand;
			if(NextCommand == CharacterCommand.None)
			{
				commandToCalculate = _context.InputData.Command;
			}

			if(CurrentState.CanExecuteNextCommand(commandToCalculate))
			{
				switch(commandToCalculate)
				{
					case CharacterCommand.None:
						if(CurrentState.IsContinuousForCommand(_context.InputData.Command))
						{
							return;
						}
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

						_attackState.SetType(NextCommand);

						if(CurrentState is AttackState)
						{
							_attackState.TryContinueCombo();
						}
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

		private void SetState(BaseCharacterState newState)
		{
			CurrentState?.OnExit();
			CurrentState = newState;
			CurrentState.OnEnter();
			NextCommand = CharacterCommand.None;
		}

		public string GetDebugString()
		{
			var str = "";
			str += $"state:   {CurrentState.GetType().Name}  complete: {CurrentState.IsComplete}\n";
			str += $"command: {_context.InputData.Command}\n";
			str += $"next command: {NextCommand}\n";
			return str;
		}
	}
}
