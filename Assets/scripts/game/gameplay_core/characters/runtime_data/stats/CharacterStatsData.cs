using System;
using dream_lib.src.reactive;
using game.gameplay_core.characters.runtime_data.bindings.stats;
using Sirenix.OdinInspector;

namespace game.gameplay_core.characters.runtime_data.stats
{
	[Serializable]
	public class CharacterStatsData
	{
		public Hp Hp { get; private set; }
		public HpMax HpMax { get; private set; }
		public ReactiveProperty<float> RecoverableHp { get; private set; }

		public Stamina Stamina { get; private set; }
		public StaminaMax StaminaMax { get; private set; }
		public ReactiveProperty<float> RecoverableStamina { get; private set; }

		[BoxGroup("Poise")]
		[ShowInInspector]
		public Poise Poise { get; private set; }
		[BoxGroup("Poise")]
		[ShowInInspector]
		public PoiseMax PoiseMax { get; private set; }
		[BoxGroup("Poise")]
		[ShowInInspector]
		public PoiseRestoreTimer PoiseRestoreTimer { get; private set; }
		[BoxGroup("Poise")]
		[ShowInInspector]
		public ReactiveProperty<float> PoiseRestoreTimerMax { get; private set; }

		public float KnockBackMultiplier { get; set; } = 1f;

		public LocomotionStatsData Locomotion { get; private set; }

		public CharacterStatsData()
		{
			Hp = new Hp();
			HpMax = new HpMax();
			RecoverableHp = new ReactiveProperty<float>();
			Stamina = new Stamina();
			StaminaMax = new StaminaMax();
			RecoverableStamina = new ReactiveProperty<float>();
			Poise = new Poise();
			PoiseMax = new PoiseMax();
			PoiseRestoreTimer = new PoiseRestoreTimer();
			PoiseRestoreTimerMax = new PoiseRestoreTimer();
			Locomotion = new LocomotionStatsData();
		}

#if UNITY_EDITOR
		[Button]
#endif

		//TODO how to rename? hp is not a stat
		public void SetStatsToMax()
		{
			Hp.Value = HpMax.Value;
			Stamina.Value = StaminaMax.Value;
			Poise.Value = PoiseMax.Value;
			PoiseRestoreTimer.Value = PoiseRestoreTimerMax.Value;
		}
	}
}
