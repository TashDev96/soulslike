using System;
using game.gameplay_core.characters.commands;
using UnityEngine;

namespace game.gameplay_core.characters.runtime_data
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
