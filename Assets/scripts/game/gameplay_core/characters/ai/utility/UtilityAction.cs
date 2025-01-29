using System;
using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.ai.utility.considerations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility
{
	[Serializable]
	public class UtilityAction
	{
		public enum ActionType
		{
			LightAttack,
			StrongAttack,
			SpecialAttack,
			Roll,
			WalkToTransform,
			KeepSafeDistance,
			GetIntoAttackDistance,
			Strafe,
			Heal
		}

		public string Id;
		public ActionType Type;
		public Direction Direction;
		public float Distance;

		[SerializeReference] [HideReferenceObjectPicker]
		public List<Consideration> Considerations = new();
	}
}
