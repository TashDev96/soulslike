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
			Heal,
			Block
		}

		public string Id;
		public ActionType Type;
		public Direction Direction;
		public float Distance;

		[SerializeReference] [HideReferenceObjectPicker]
		public List<Consideration> Considerations = new();

		public bool HasInertia;
		[ShowIf("HasInertia")]
		public float InertiaDuration;
		[ShowIf("HasInertia")]
		public AnimationCurve InertiaCurve;

		[ShowIf("@UtilityAiEditorHelper.DebugEnabled")]
		[ShowInInspector]
		[GUIColor("DebugColor")]
		public float DebugWeightCache { get; set; }
		[field: SerializeField]
		[field: PropertyOrder(-1)]
		public Color DebugColor { get; set; }
		public float InertiaTimer { get; set; }
	}
}
