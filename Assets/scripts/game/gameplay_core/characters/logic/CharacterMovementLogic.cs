using game.gameplay_core.characters.bindings;
using UnityEngine;

namespace game.gameplay_core.characters.logic
{
	public class CharacterMovementLogic
	{
		public struct Context
		{
			public Transform CharacterTransform;
			public CharacterController UnityCharacterController;
			public IsDead IsDead { get; set; }
		}

		private Context _context;
		private Vector3 _fallVelocity;

		public void SetContext(Context context)
		{
			_context = context;
			_context.IsDead.OnChanged += HandleDeath;
		}

		private void HandleDeath(bool isDead)
		{
			_context.UnityCharacterController.enabled = !isDead;
		}

		public void Update(float deltaTime)
		{
			if(_context.IsDead.Value)
			{
				return;
			}

			if(!_context.UnityCharacterController.isGrounded)
			{
				_fallVelocity += Physics.gravity * deltaTime;
				_context.UnityCharacterController.Move(_fallVelocity * deltaTime);
			}
			else
			{
				_fallVelocity = Vector3.zero;
			}
		}

		public void Move(Vector3 vector)
		{
			_context.UnityCharacterController.Move(vector);
		}
	}
}
