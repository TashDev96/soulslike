using dream_lib.src.reactive;
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
		public ReactiveProperty<float> WalkSpeed { get; set; }
		public ReactiveProperty<float> RotationSpeed { get; set; }
	}
}
