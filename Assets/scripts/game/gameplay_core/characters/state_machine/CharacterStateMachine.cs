using System;

namespace game.gameplay_core.characters.state_machine
{
	public class CharacterStateMachine
	{
		private CharacterCommand _nextCommand;
		private readonly CharacterContext _context;
		public BaseCharacterState CurrentState { get; private set; }

		private IdleState _idleState;
		private WalkState _walkState;
		private AttackState _attackState;

		public CharacterStateMachine(CharacterContext characterContext)
		{
			_context = characterContext;

			_idleState = new IdleState(_context);
			_walkState = new WalkState(_context);
			_attackState = new AttackState(_context);
			SetState(_idleState);
		}

		public void Update(float deltaTime)
		{
			TryRememberNextCommand();

			CurrentState.Update(deltaTime);
				//

			TryChangeState();
		}

		private void TryRememberNextCommand()
		{
			var inputCommand = _context.InputData.Command;

			if(_nextCommand == CharacterCommand.None)
			{
				if(CheckIsContinuousCommand(inputCommand))
				{
					return;
				}

				if(CurrentState.IsReadyToRememberNextCommand)
				{
					_nextCommand = inputCommand;
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
			var commandToCalculate = _nextCommand;
			if (_nextCommand == CharacterCommand.None)
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

						_attackState.SetType(_nextCommand);

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
				_nextCommand = CharacterCommand.None;
			}
			return;
		}

		private void SetState(BaseCharacterState newState)
		{
			CurrentState = newState;
			CurrentState.OnEnter();
			_nextCommand = CharacterCommand.None;
		}

		public string GetDebugString()
		{
			var str = "";
			str += $"state:   {CurrentState.GetType().Name}  complete: {CurrentState.IsComplete}\n";
			str += $"command: {_context.InputData.Command}\n";
			str += $"next command: {_nextCommand}\n";
			return str;
		}
	}
}
