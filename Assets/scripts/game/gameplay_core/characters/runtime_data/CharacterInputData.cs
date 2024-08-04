using System;
using UnityEngine;

namespace game.gameplay_core.characters
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
		public bool HasDirectionInput => DirectionWorld.sqrMagnitude > 0;
	}
}
