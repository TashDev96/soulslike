using game.gameplay_core.characters.config.animation;
using UnityEngine;

namespace game.gameplay_core.characters.config
{
	[CreateAssetMenu(menuName = "Configs/TestAnimationConfig")]
	public class TestAnimationConfig : ScriptableObject
	{
		public AnimationConfig Config;
	}
}
