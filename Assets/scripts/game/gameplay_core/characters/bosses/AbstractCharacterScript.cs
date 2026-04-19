using UnityEngine;

namespace game.gameplay_core.characters.bosses
{
	public abstract class AbstractCharacterScript : MonoBehaviour
	{
		public abstract void SetContext(CharacterContext context);
	}
}
