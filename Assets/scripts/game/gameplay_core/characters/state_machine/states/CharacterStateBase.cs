using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.state_machine.states
{
	public abstract class CharacterStateBase
	{
		protected CharacterContext _context;
		public bool IsComplete { get; protected set; }
		public bool IsReadyToRememberNextCommand { get; set; }
		public virtual bool CanInterruptByStagger => true;

		protected CharacterStateBase(CharacterContext context)
		{
			_context = context;
		}

		public abstract void Update(float deltaTime);

		public virtual void HandleNextInput(CharacterCommand input, out bool readyToRemember)
		{
			readyToRemember = false;
		}

		public virtual void OnEnter()
		{
		}

		public virtual void OnExit()
		{
		}

		public virtual void OnInterrupt()
		{
		}

		public virtual bool CheckIsReadyToChangeState()
		{
			return IsComplete;
		}

		protected void RotateCharacter(Vector3 toDirection, float speed, float deltaTime)
		{
			var targetRotation = Quaternion.LookRotation(toDirection);
			_context.Transform.rotation = Quaternion.RotateTowards(_context.Transform.rotation, targetRotation, speed * deltaTime);
		}

		public virtual bool TryContinueWithCommand(CharacterCommand nextCommand)
		{
			return false;
		}
	}
}
