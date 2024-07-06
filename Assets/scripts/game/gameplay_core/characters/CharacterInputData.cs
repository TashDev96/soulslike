using System;
using UnityEngine;

namespace game.gameplay_core.character
{
	[Serializable]
	public class CharacterInputData
	{
		public CharacterCommand Command;
		public float LastCommandTime;
		public bool HoldBlock;
		public bool HoldRun;

		public Vector3 DirectionLocal;
		public Vector3 DirectionWorld;
	}
}
