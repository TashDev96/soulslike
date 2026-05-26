using dream_lib.src.reactive;

namespace game
{
	public class Shortcuts
	{
		public static ReactivePropertyWithDelayedDisplayInt PlayerSoftCurrency => GameStaticContext.Instance.PlayerSave.SoftCurrency;
	}
}
