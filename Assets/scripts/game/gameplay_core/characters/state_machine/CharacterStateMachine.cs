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

		public CharacterStateMachine(CharacterContext characterContext)
		{
			_context = characterContext;

			_idleState = new IdleState(_context);
			_walkState = new WalkState(_context);

			CurrentState = new IdleState(_context)
			{
				Context = _context
			};
		}

		public void Update(float deltaTime)
		{
			TryRememberNextCommand();

			CurrentState.Update(deltaTime);

			TryChangeState();
		}

		private void TryRememberNextCommand()
		{
			if(_nextCommand == CharacterCommand.None)
			{
				if(CurrentState.IsContinuousForCommand(_context.InputData.Command))
				{
					return;
				}

				if(CurrentState.IsReadyToRememberNextCommand)
				{
					_nextCommand = _context.InputData.Command;
				}
			}
		}

		private void TryChangeState()
		{
			if(CurrentState.TryChangeStateByCustomLogic(out var newState))
			{
				SetState(newState);
				return;
			}

			if(CurrentState.IsComplete)
			{
				switch(_nextCommand)
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
						break;
					case CharacterCommand.StrongAttack:
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
			}
			return;
		}

		private void SetState(BaseCharacterState newState)
		{
			CurrentState = newState;
			_nextCommand = CharacterCommand.None;
		}
	}
}
