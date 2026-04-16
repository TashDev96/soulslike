using dream_lib.src.reactive;
using dream_lib.src.ui;
using game.gameplay_core.characters;
using game.ui;
using UnityEngine;

namespace game.gameplay_core.ui
{
	public class UiLocationHUD : MonoBehaviour
	{
		public struct Context
		{
			public CharacterDomain Player;
			public ReactiveCommand<float> LocationUiUpdate { get; set; }
		}

		[SerializeField]
		private PlayerHpBar _healthBar;
		[SerializeField]
		private UiBar _staminaBar;
		[SerializeField]
		private UiInteractionPrompt _uiInteractionPrompt;
		[SerializeField]
		private UiEquippedItems _equippedItems;
		[SerializeField]
		private BossHpBar _bossHealthBar;

		public void SetContext(Context context)
		{
			_healthBar.SetContext(new PlayerHpBar.Context
			{
				HealthLogic = context.Player.Context.HealthLogic,
				Current = context.Player.ExternalData.Stats.Hp,
				Max = context.Player.ExternalData.Stats.HpMax,
				RecoverableAmount = context.Player.ExternalData.Stats.RecoverableHp,
				CustomUpdate = context.LocationUiUpdate
			});

			_staminaBar.SetContext(new UiBar.Context
			{
				Current = context.Player.ExternalData.Stats.Stamina,
				Max = context.Player.ExternalData.Stats.StaminaMax,
				RecoverableAmount = context.Player.ExternalData.Stats.RecoverableStamina,
				CustomUpdate = context.LocationUiUpdate
			});

			_uiInteractionPrompt.SetContext(new UiInteractionPrompt.Context
			{
				TriggersEnteredByPlayer = context.Player.ExternalData.EnteredTriggers,
				Player = context.Player
			});

			_equippedItems.SetContext(new UiEquippedItems.Context
			{
				Player = context.Player
			});

			LocationStaticContext.Instance.CurrentlyFightingBoss.OnChanged += HandleBossStarted;
			_bossHealthBar.gameObject.SetActive(false);
		}

		private void HandleBossStarted(CharacterDomain boss)
		{
			if(boss != null)
			{
				_bossHealthBar.SetContext(boss);
			}
			else
			{
				_bossHealthBar.gameObject.SetActive(false);
			}
		}
	}
}
