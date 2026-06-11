using dream_lib.src.reactive;

namespace game.gameplay_core.characters.logic
{
	public class FlyingLogic
	{
		private CharacterContext _context;

		public ReactiveProperty<int> MaxFlapsCount { get; private set; } = new();
		public ReactiveProperty<int> FlapsLeftCount { get; private set; } = new();

		public void SetContext(CharacterContext context)
		{
			_context = context;
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
	}
}
