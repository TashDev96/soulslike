using UnityEngine;

namespace game.gameplay_core.characters.state_machine
{
	public class DeadState : BaseCharacterState
	{
		public DeadState(CharacterContext context) : base(context)
		{
		}

		public override void Update(float deltaTime)
		{
		}

		public override void OnEnter()
		{
			_context.Transform.Rotate(_context.Transform.right, 90f, Space.World);
			_context.DeadStateRoot.SetActive(true);
		}

		public override void OnExit()
		{
			_context.Transform.rotation = Quaternion.identity;
			_context.DeadStateRoot.SetActive(false);
		}
	}
}
