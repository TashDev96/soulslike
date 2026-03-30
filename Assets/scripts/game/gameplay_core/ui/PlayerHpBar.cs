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
		private Slider _slider;
		[SerializeField]
		private Image _topper;

		[SerializeField]
		private Color _blinkColor = Color.white;

		[SerializeField]
		private float _blinkDuration = 0.3f;

		[SerializeField]
		private int _blinkCount = 3;

		private Context _context;
		private float _currentTargetValue = 1f;
		private float _slowFillerValue = 1f;
		private Color _defaultSlowColor;
		private Coroutine _blinkCoroutine;

		public void SetContext(Context context)
		{
			_context = context;

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

		private void Awake()
		{
			_defaultSlowColor = _fillerSlow.color;
		}

		public void Reset()
		{
			StopAllCoroutines();
			_currentTargetValue = 1f;
			_slowFillerValue = 1f;
		}

		private void CustomUpdate(float deltaTime)
		{
			RefreshUI();
		}

		private void HandleValueChanged(float value)
		{
			var prevValue = _currentTargetValue;
			_currentTargetValue = Mathf.Clamp01(value / _context.Max.Value);

			if(_context.RecoverableAmount != null)
			{
				_slowFillerValue = Mathf.Clamp01((value + _context.RecoverableAmount.Value) / _context.Max.Value);
			}
			else
			{
				_slowFillerValue = _currentTargetValue;
			}

			if(_currentTargetValue < prevValue)
			{
				if(_blinkCoroutine != null)
				{
					StopCoroutine(_blinkCoroutine);
				}
				if(gameObject.activeInHierarchy)
				{
					_blinkCoroutine = StartCoroutine(BlinkCoroutine());
				}
			}
		}

		private void HandleRecoverableValueChanged(float value)
		{
			_slowFillerValue = Mathf.Clamp01((_context.Current.Value + value) / _context.Max.Value);
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
