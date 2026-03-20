using DG.Tweening;
using dream_lib.src.extensions;
using game.gameplay_core.damage_system;
using TMPro;
using UnityEngine;

namespace game.gameplay_core.characters.view.ui
{
	public class CharacterDamageCounterUi : MonoBehaviour
	{
		[SerializeField]
		private float _displayDuration = 1.5f;

		[SerializeField]
		private float _fadeOutDuration = 0.5f;

		[SerializeField]
		private TMP_Text _text;

		public CharacterContext _context;

		private float _currentDamageSum;
		private Tween _hideTween;
		private Tween _popTween;
		private Tween _fadeTween;

		public void SetContext(CharacterContext context)
		{
			_context = context;

			if(_context.ApplyDamage != null)
			{
				_context.ApplyDamage.OnExecute += HandleDamageApplied;
			}

			_text.gameObject.SetActive(false);
		}

		private void HandleDamageApplied(DamageInfo info)
		{
			if(info.DamageAmount <= 0)
			{
				return;
			}

			_currentDamageSum += info.DamageAmount;
			_text.SetText(_currentDamageSum.RoundFormat(5));

			// Kill existing tweens to reset state
			_hideTween?.Kill();
			_popTween?.Kill();
			_fadeTween?.Kill();

			// Reveal and "pop" animation
			if(!_text.gameObject.activeSelf)
			{
				_text.gameObject.SetActive(true);
				_text.alpha = 1f;
				_text.transform.localScale = Vector3.one * 0.3f;
				_popTween = _text.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
			}
			else
			{
				_text.alpha = 1f;
				_text.transform.localScale = Vector3.one * 0.3f;
				_popTween = _text.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
			}

			// Schedule hide
			_hideTween = DOVirtual.DelayedCall(_displayDuration, HideDamageText);
		}

		private void HideDamageText()
		{
			_fadeTween = _text.DOFade(0f, _fadeOutDuration).OnComplete(() =>
			{
				_text.gameObject.SetActive(false);
				_currentDamageSum = 0;
			});
		}

		private void OnDestroy()
		{
			if(_context.ApplyDamage != null)
			{
				_context.ApplyDamage.OnExecute -= HandleDamageApplied;
			}
			_hideTween?.Kill();
			_popTween?.Kill();
			_fadeTween?.Kill();
		}
	}
}
