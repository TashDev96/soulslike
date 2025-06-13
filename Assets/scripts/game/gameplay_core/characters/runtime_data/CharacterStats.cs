using System;
using dream_lib.src.reactive;
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

		[BoxGroup("Poise")]
		public Poise Poise;
		[BoxGroup("Poise")]
		public PoiseMax PoiseMax;
		[BoxGroup("Poise")]
		public PoiseRestoreTimer PoiseRestoreTimer;
		[BoxGroup("Poise")]
		public ReactiveProperty<float> PoiseRestoreTimerMax;

		public CharacterStats()
		{
			Hp = new Hp();
			HpMax = new HpMax();
			Stamina = new Stamina();
			StaminaMax = new StaminaMax();
			Poise = new Poise();
			PoiseMax = new PoiseMax();
			PoiseRestoreTimer = new PoiseRestoreTimer();
			PoiseRestoreTimerMax = new PoiseRestoreTimer();
		}

#if UNITY_EDITOR
		[Button]
		private void SetStatsToMax()
		{
			Hp.Value = HpMax.Value;
			Stamina.Value = StaminaMax.Value;
			Poise.Value = PoiseMax.Value;
			PoiseRestoreTimer.Value = PoiseRestoreTimerMax.Value;
			EditorUtility.SetDirty(Selection.activeObject);
		}
#endif
	}
}
