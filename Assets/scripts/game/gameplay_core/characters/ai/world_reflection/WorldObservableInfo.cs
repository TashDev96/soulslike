using System.Collections.Generic;
using dream_lib.src.reactive;

namespace game.gameplay_core.characters.ai.world_reflection
{
	public class WorldObservableInfo
	{
		public List<VisualInfoPoint> VisualInfoPoints = new();
		public ReactiveCommand<SoundInfo> PropagateSoundInfo = new();

		public void RegisterVisualInfo(VisualInfoPoint visualInfoPoint)
		{
			VisualInfoPoints.Add(visualInfoPoint);
		}
	}
}
