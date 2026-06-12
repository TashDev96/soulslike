using dream_lib.src.reactive;
using game.gameplay_core.characters.state_machine.states;

namespace game.gameplay_core.characters.logic
{
	public class FlyingLogic
	{
		private CharacterContext _context;

		public ReactiveProperty<int> MaxFlapsCount { get; } = new();
		public ReactiveProperty<int> FlapsLeftCount { get; } = new();

		public void SetContext(CharacterContext context)
		{
			_context = context;
			_context.CurrentState.OnChanged += HandleStateChanged;
			MaxFlapsCount.Value = 3;
		}

		public void CustomUpdate(float deltaTime)
		{
			var restoreFlaps = !_context.IsBirdMode.Value && !_context.IsFalling.Value;
			if(_context.IsBirdMode.Value)
			{
				restoreFlaps |= _context.Logic.MovementLogic.IsGrounded;
			}
			if(restoreFlaps)
			{
				FlapsLeftCount.Value = MaxFlapsCount.Value;
			}
		}

		public bool TryFlap()
		{
			if(FlapsLeftCount.Value > 0)
			{
				FlapsLeftCount.Value--;
				return true;
			}
			return false;
		}

		private void HandleStateChanged(CharacterStateBase state)
		{
			if(state is PlungeAttackState)
			{
				FlapsLeftCount.Value = MaxFlapsCount.Value;
			}
		}
	}
}
