using Sirenix.OdinInspector.Editor;

namespace game.gameplay_core.characters.ai
{
	[OdinDontRegister]
	public class SubUtilityFight : SubUtilityBase
	{
		//chain of goals
		//chain examples:
		//multiple attacks
		//jump back then heal
		//jump attack then roll back

		//input: enemy
		//input: self stats
		//input: inventory

		//input: fight history data

		//list of attacks
		//list of defences
		//list of movement
		//list of stupidities

		public void HandleCharacterUpdate(float deltaTime)
		{
		}
	}
}
