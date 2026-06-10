namespace game.gameplay_core.characters.logic
{
	public class FlyingLogic
	{
		private CharacterContext _context;

		public int FlapsLeftCount { get; private set; }

		public void SetContext(CharacterContext context)
		{
			_context = context;
		}

		public void CustomUpdate(float deltaTime)
		{
			if(!_context.FlyingMode.Value && !_context.IsFalling.Value)
			{
				FlapsLeftCount = 2;
			}
		}

		public bool TryFlap()
		{
			if(FlapsLeftCount > 0)
			{
				FlapsLeftCount--;
				return true;
			}
			return false;
		}
	}
}
