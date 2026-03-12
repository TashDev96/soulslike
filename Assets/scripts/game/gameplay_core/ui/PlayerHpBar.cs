using System;
using System.Collections;
using dream_lib.src.reactive;
using game.gameplay_core.characters.logic;
using UnityEngine;
using UnityEngine.UI;

namespace game.gameplay_core.ui
{
	public class PlayerHpBar : MonoBehaviour
	{
		public struct Context
		{
			public IReadOnlyReactiveProperty<float> Max;
			public IReadOnlyReactiveProperty<float> Current;
			public IReadOnlyReactiveProperty<float> RecoverableAmount;
			public HealthLogic HealthLogic;
			public ReactiveCommand<float> CustomUpdate { get; set; }
		}

		[Header("References")]
		[SerializeField]
		private Image _fillerFast;
		[SerializeField]
		private Image _fillerSlow;
		[SerializeField]
		private CanvasGroup _alphaContainer;
		[SerializeField]
		private Slider _slider;
		[SerializeField]
		private Image _topper;
		
		[SerializeField]
		private Color _blinkColor = Color.white;

		[SerializeField]
		private float _blinkDuration = 0.3f;

		[SerializeField]
		private int _blinkCount = 3;

		[Header("Settings - Animation")]
		[SerializeField]
		private float _alphaAppearDuration = 1f;

		[Header("Settings - Visibility")]
		[SerializeField]
		private bool _autoHide;
		[SerializeField]
		private float _notLockedShowDuration;
		private Context _context;
		private float _currentTargetValue = 1f;
		private float _slowFillerValue = 1f;
		private float _showTime;
		private bool _isAppearing;
		private Color _defaultSlowColor;
		private Coroutine _blinkCoroutine;

		private void Awake()
		{
			_defaultSlowColor = _fillerSlow.color;
		}

		public void SetContext(Context context)
		{
			_context = context;

			if(_autoHide)
			{
				gameObject.SetActive(false);
			}

			_showTime = _notLockedShowDuration;

			_currentTargetValue = Mathf.Clamp01(_context.Current.Value / _context.Max.Value);
			_slowFillerValue = _context.RecoverableAmount != null
				? Mathf.Clamp01((_context.Current.Value + _context.RecoverableAmount.Value) / _context.Max.Value)
				: _currentTargetValue;

			_context.Current.OnChanged += HandleValueChanged;
			if(_context.RecoverableAmount != null)
			{
				_context.RecoverableAmount.OnChanged += HandleRecoverableValueChanged;
			}
			_context.CustomUpdate.OnExecute += CustomUpdate;

		}

		public void Reset()
		{
			StopAllCoroutines();
			_currentTargetValue = 1f;
			_slowFillerValue = 1f;
			if(_autoHide)
			{
				_isAppearing = false;
				_showTime = 2;
				_alphaContainer.alpha = 0;
			}
		}

		private void CustomUpdate(float deltaTime)
		{
			if(_autoHide)
			{
				UpdateVisibility(deltaTime);
			}

			RefreshUI();
		}

		private void HandleValueChanged(float value)
		{
			var prevValue = _currentTargetValue;
			_currentTargetValue = Mathf.Clamp01(value / _context.Max.Value);

			if(_currentTargetValue < prevValue)
			{
				if(_blinkCoroutine != null)
				{
					StopCoroutine(_blinkCoroutine);
				}
				_blinkCoroutine = StartCoroutine(BlinkCoroutine());
			}
			if(_context.RecoverableAmount != null)
			{
				_slowFillerValue = Mathf.Clamp01((value + _context.RecoverableAmount.Value) / _context.Max.Value);
			}
			else
			{
				_slowFillerValue = _currentTargetValue;
			}

			if(_autoHide)
			{
				HandleAutoShow();
			}
		}

		private void HandleRecoverableValueChanged(float value)
		{
			_slowFillerValue = Mathf.Clamp01((_context.Current.Value + value) / _context.Max.Value);

			if(_autoHide)
			{
				HandleAutoShow();
			}
		}
		
		private IEnumerator BlinkCoroutine()
		{
			var singleBlinkDuration = _blinkDuration / _blinkCount;
			var halfBlinkDuration = singleBlinkDuration * 0.5f;

			 

			for(var i = 0; i < _blinkCount; i++)
			{
				_fillerSlow.color = _blinkColor;
				yield return new WaitForSeconds(halfBlinkDuration);
				_fillerSlow.color = _defaultSlowColor;
				yield return new WaitForSeconds(halfBlinkDuration);
			}

			_fillerSlow.color = _defaultSlowColor;
			_blinkCoroutine = null;
		}


		private void RefreshUI()
		{
			ApplyValueToFiller(_fillerFast, _currentTargetValue);
			ApplyValueToFiller(_fillerSlow, _slowFillerValue);

			if(_slider != null)
			{
				_slider.value = _currentTargetValue;
				if(_topper != null)
				{
					_topper.enabled = _currentTargetValue > 0;
				}
			}
		}

		private void HandleAutoShow()
		{
			var hasValue = _currentTargetValue < 1 && _currentTargetValue > 0;
			if(hasValue && !gameObject.activeSelf)
			{
				if(_alphaContainer != null)
				{
					_alphaContainer.alpha = 0;
				}
				gameObject.SetActive(true);
			}

			_isAppearing = true;
			_showTime = 0f;
		}

		private void UpdateVisibility(float deltaTime)
		{
			if(_alphaContainer == null)
			{
				return;
			}

			var isVisible = _showTime < _notLockedShowDuration && _currentTargetValue > 0;
			isVisible |= _currentTargetValue <= 0f && _slowFillerValue > 0f;

			var step = deltaTime / Mathf.Max(0.001f, _alphaAppearDuration);

			if(isVisible)
			{
				if(_isAppearing)
				{
					_alphaContainer.alpha = Mathf.MoveTowards(_alphaContainer.alpha, 1f, step);
					if(_alphaContainer.alpha >= 1f)
					{
						_isAppearing = false;
					}
				}
				else
				{
					_showTime += deltaTime;
				}
			}
			else
			{
				_alphaContainer.alpha = Mathf.MoveTowards(_alphaContainer.alpha, 0f, step);
				if(_alphaContainer.alpha <= 0f)
				{
					gameObject.SetActive(false);
				}
			}
		}

		private void ApplyValueToFiller(Image filler, float value)
		{
			if(filler == null)
			{
				return;
			}

			if(filler.type == Image.Type.Filled)
			{
				filler.fillAmount = value;
			}
			else
			{
				filler.transform.localScale = new Vector3(value, 1, 1);
			}
		}
	}
}
