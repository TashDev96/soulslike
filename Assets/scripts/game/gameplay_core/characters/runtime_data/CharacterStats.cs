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
		public Hp Hp { get; private set; }
		public HpMax HpMax { get; private set; }

		public Stamina Stamina { get; private set; }
		public StaminaMax StaminaMax { get; private set; }

		[BoxGroup("Poise")]
		public Poise Poise { get; private set; }
		[BoxGroup("Poise")]
		public PoiseMax PoiseMax { get; private set; }
		[BoxGroup("Poise")]
		public PoiseRestoreTimer PoiseRestoreTimer { get; private set; }
		[BoxGroup("Poise")]
		public ReactiveProperty<float> PoiseRestoreTimerMax { get; private set; }

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
#endif
		
		public void SetStatsToMax()
		{
			Hp.Value = HpMax.Value;
			Stamina.Value = StaminaMax.Value;
			Poise.Value = PoiseMax.Value;
			PoiseRestoreTimer.Value = PoiseRestoreTimerMax.Value;
		}
	}
}
