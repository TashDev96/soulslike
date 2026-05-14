using System.Collections;
using dream_lib.ui;
using game.gameplay_core.characters;
using game.gameplay_core.characters.view.ui;
using game.gameplay_core.location;
using TMPro;
using UnityEngine;

namespace game.gameplay_core.ui
{
	public class BossHpBar : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _bossName;

		[SerializeField]
		private UiBar _bar;
		[SerializeField]
		private CharacterDamageCounterUi _damageCounterUi;
		private CharacterDomain _boss;

		public void SetContext(CharacterDomain bossDomain)
		{
			gameObject.SetActive(true);
			_bossName.text = bossDomain.name;
			_boss = bossDomain;

			_bar.SetContext(new UiBar.Context
			{
				Current = bossDomain.Context.CharacterStats.Hp.Current,
				Max = bossDomain.Context.CharacterStats.Hp.Max,
				RecoverableAmount = bossDomain.Context.CharacterStats.Hp.Recoverable,
				CustomUpdate = LocationStaticContext.Instance.LocationUiUpdate
			});

			_damageCounterUi.SetContext(_boss.Context);

			bossDomain.Context.CharacterStats.Hp.Current.OnChanged += HandleBossHpChanged;
		}

		private void HandleBossHpChanged(float newHp)
		{
			if(newHp <= 0f)
			{
				_boss.Context.CharacterStats.Hp.Current.OnChanged -= HandleBossHpChanged;
				StartCoroutine(Disappear());
			}
		}

		private IEnumerator Disappear()
		{
			yield return new WaitForSeconds(2f);
			gameObject.SetActive(false);
		}
	}
}
