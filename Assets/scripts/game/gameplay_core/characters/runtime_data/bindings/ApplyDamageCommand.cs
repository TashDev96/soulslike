using dream_lib.src.reactive;
using game.gameplay_core.damage_system;

namespace game.gameplay_core.characters.runtime_data.bindings
{
	public class ApplyDamageCommand : ReactiveCommand<DamageInfo>
	{
		public override void Execute(DamageInfo value)
		{
			base.Execute(value);
		}
	}
}
