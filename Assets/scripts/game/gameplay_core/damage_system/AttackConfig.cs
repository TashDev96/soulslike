using System;
using System.Collections.Generic;
using Animancer;
using game.editor;
using game.gameplay_core.inventory.item_configs;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using dream_lib.src.utils.editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

namespace game.gameplay_core.damage_system
{
	using game.gameplay_core.characters.config.animation;

	[Serializable]
	public class AttackConfig
	{
		[field: SerializeField]
		public AnimationConfig AnimationConfig { get; private set; } = new();

		[field: SerializeField]
		public ClipTransition Animation { get; private set; }

		[field: SerializeField]
		public bool IsRangedAttack { get; private set; }

		[field: ValueDropdown("@AddressableAssetNames.ProjectilePrefabs")]
		[field: SerializeField]
		[field: ShowIf(nameof(IsRangedAttack))]
		public string ProjectilePrefabNames { get; private set; }
		[field: SerializeField]
		[field: ShowIf(nameof(IsRangedAttack))]
		public float MaxProjectileHorizontalAngleCorrection { get; private set; }

		[field: SerializeField]
		public float BaseDamage { get; private set; }
		[field: SerializeField]
		public float StaminaCost { get; private set; } = 10;
		[field: SerializeField]
		public int AttackDeflectionRatingBonus { get; private set; }

		[ShowInInspector]
		public float Duration => Animation.Clip ? Animation.Clip.length / Animation.Speed : 0.1f;


		[field: SerializeField]
		public AnimationCurve ForwardMovement { get; private set; }

		[field: BoxGroup("Ai Data")]
		[field: SerializeField]
		public float Range { get; set; } = 1f;


#if UNITY_EDITOR


		[OnInspectorGUI]
		private void DrawCustomHitsInspector()
		{
			var weaponKey = GetWeaponPrefabKeyFromSelection();
			
			//TODO: use weapon in animationpewview
			AnimationConfig.WeaponForPreview = weaponKey;
			
		}


		private string GetWeaponPrefabKeyFromSelection()
		{
			if(Selection.activeObject is WeaponItemConfig weaponConfig)
			{
				return weaponConfig.WeaponPrefabName;
			}
			return null;
		}

#endif //
		
	}
}
