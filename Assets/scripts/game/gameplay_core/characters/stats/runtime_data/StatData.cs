using dream_lib.src.reactive;
using game.gameplay_core.characters.stats.config;

namespace game.gameplay_core.characters.stats.runtime_data
{
	public class StatData
	{
		public StatKey Id;
		public StatConfig Config;

		public ReactiveProperty<float> Current = new();
		public ReactiveProperty<float> Max = new();

		public bool IsHidden = false;

		public float Value
		{
			get => Current.Value;
			set => Current.Value = value;
		}

		public float MaxValue
		{
			get => Max.Value;
			set => Max.Value = value;
		}

		public StatData(StatKey id, StatConfig config)
		{
			Id = id;
			Config = config;
		}

		public void SetToMax()
		{
			Current.Value = MaxValue;
		}
	}
}
