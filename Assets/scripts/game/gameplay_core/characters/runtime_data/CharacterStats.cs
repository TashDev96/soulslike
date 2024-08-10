using System;
using game.gameplay_core.characters.runtime_data.bindings.stats;
using Sirenix.OdinInspector;
using UnityEditor;

namespace game.gameplay_core.characters.runtime_data
{
	[Serializable]
	public class CharacterStats
	{
		public Hp Hp;
		public HpMax HpMax;

		public Stamina Stamina;
		public StaminaMax StaminaMax;

		public Poise Poise;
		public PoiseMax PoiseMax;
		public PoiseRestoreTime PoiseRestoreTime;

#if UNITY_EDITOR
		[Button]
		private void SetStatsToMax()
		{
			Hp.Value = HpMax.Value;
			Stamina.Value = StaminaMax.Value;
			Poise.Value = PoiseMax.Value;
			EditorUtility.SetDirty(Selection.activeObject);
		}
#endif
	}
}
