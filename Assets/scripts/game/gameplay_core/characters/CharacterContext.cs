using UnityEngine;

namespace game.gameplay_core.characters
{
	public class CharacterContext
	{
		public CharacterInputData InputData;
		public Transform Transform;

		public CharacterContext(Transform transform)
		{
			Transform = transform;
			InputData = new CharacterInputData();
		}

		//inputs

		//stats

		//hp
		//stamina
		//
	}
}
