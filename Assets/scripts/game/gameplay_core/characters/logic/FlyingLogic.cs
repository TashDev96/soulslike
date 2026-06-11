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
			if(!_context.FlyingMode.Value && !_context.IsFalling.Value)
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
