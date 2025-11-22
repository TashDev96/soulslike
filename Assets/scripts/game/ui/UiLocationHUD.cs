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
		private UiBar _healthBar;
		[SerializeField]
		private UiBar _staminaBar;
		[SerializeField]
		private UiInteractionPrompt _uiInteractionPrompt;

		public void SetContext(Context context)
		{
			_healthBar.SetContext(new UiBar.Context
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
			
			_uiInteractionPrompt.SetContext(new UiInteractionPrompt.Context()
			{
				TriggersEnteredByPlayer = context.Player.ExternalData.EnteredTriggers,
				Player = context.Player,
			});
		}
	}
}
