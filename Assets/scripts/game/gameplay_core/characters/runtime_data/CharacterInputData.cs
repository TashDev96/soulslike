using System;
using game.gameplay_core.characters.commands;
using game.gameplay_core.damage_system;
using UnityEngine;
using UnityEngine.Serialization;

namespace game.gameplay_core.characters.runtime_data
{
	[Serializable]
	public class CharacterInputData
	{
		public CharacterCommand Command;
		public float LastCommandTime;
		public bool HoldBlock;
		public bool HoldRun;

		public Vector3 DirectionWorld;
		public int SpecialAttackIndex;
		public bool HasDirectionInput => DirectionWorld.sqrMagnitude > 0;
		public AttackConfig ForcedAttackConfig;
	}
}
