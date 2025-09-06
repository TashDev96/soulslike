using dream_lib.src.reactive;
using dream_lib.src.ui;
using game.gameplay_core.characters;
using UnityEngine;

namespace game.ui
{
	public class UiLocationHUD : MonoBehaviour
	{
		public struct Context
		{
			public CharacterDomain Player;
			public ReactiveCommand<float> LocationUiUpdate { get; set; }
		}

		[SerializeField]
		private UiBar _healthBat;
		[SerializeField]
		private UiBar _staminaBar;

		public void SetContext(Context context)
		{
			_healthBat.SetContext(new UiBar.Context
			{
				Current = context.Player.ExternalData.Stats.Hp,
				Max = context.Player.ExternalData.Stats.HpMax,
				CustomUpdate = context.LocationUiUpdate
			});
			
			_staminaBar.SetContext(new UiBar.Context
			{
				Current = context.Player.ExternalData.Stats.Stamina,
				Max = context.Player.ExternalData.Stats.StaminaMax,
				CustomUpdate = context.LocationUiUpdate
			});
		}
	}
}
