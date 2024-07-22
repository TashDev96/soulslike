using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public abstract class BaseCharacterState
	{
		protected CharacterContext _context;
		public bool IsComplete { get; protected set; }
		public bool IsReadyToRememberNextCommand { get; set; }

		protected BaseCharacterState(CharacterContext context)
		{
			_context = context;
		}

		protected void RotateCharacter(Vector3 toDirection, float speed, float deltaTime)
		{
			var targetRotation = Quaternion.LookRotation(toDirection);
			_context.Transform.rotation = Quaternion.RotateTowards(_context.Transform.rotation, targetRotation, speed * deltaTime);
		}

		public abstract void Update(float deltaTime);

		public virtual bool TryChangeStateByCustomLogic(out BaseCharacterState newState)
		{
			newState = null;
			return false;
		}

		public virtual void HandleNextInput(CharacterCommand input, out bool readyToRemember)
		{
			readyToRemember = false;
		}

		public virtual bool IsContinuousForCommand(CharacterCommand command)
		{
			return false;
		}

		public virtual void OnEnter()
		{
			
		}
	}
}
