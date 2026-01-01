using System.Collections.Generic;

namespace game.gameplay_core.damage_system
{
	public interface IHitConfig
	{
		float StartTime { get; }
		float EndTime { get; }
		List<bool> InvolvedColliders { get; }
		bool FriendlyFire { get; }
		float DamageMultiplier { get; }
		float PoiseDamage { get; }
	}
}
