using System;
using System.Collections.Generic;
using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.ai.considerations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	[Serializable]
	public class UtilityAction
	{
		[SerializeReference, HideReferenceObjectPicker]
		public Consideration[] Considerations = Array.Empty<Consideration>();

		public ActionType Action;
		public Direction Direction;
		
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
			Heal,
		}
	}
}
