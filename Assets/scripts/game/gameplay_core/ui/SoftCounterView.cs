using dream_lib.ui.animations;
using game.gameplay_core.location;
using TMPro;
using UnityEngine;

namespace game.gameplay_core.ui
{
	public class SoftCounterView : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private TMP_Text _incomeText;

		[SerializeField]
		private float _incomeClearDelay = 1f;

		[SerializeReference]
		private UiAnimationBase[] _mainTextIncomeAnimations;

		[SerializeReference]
		private UiAnimationBase[] _incomeTextIncomeAnimations;

		private int _displayedCount;
		private float _displayedCountFloat;
		private int _displayedCountTarget;
		private int _currentShownIncome;
		private bool _waitingToClearIncome;
		private float _clearIncomeTimer;

		public void Initialize()
		{
			_displayedCount = Shortcuts.PlayerSoftCurrency.RealValue;
			_displayedCountFloat = _displayedCount;
			_displayedCountTarget = _displayedCount;
			_text.text = _displayedCount.ToString();

			Shortcuts.PlayerSoftCurrency.OnDisplayingValueChanged += HandleDisplayingValueChanged;
			LocationStaticContext.Instance.LocationUiUpdate.OnExecute += CustomUpdate;

			foreach(var newIncomeAnimation in _mainTextIncomeAnimations)
			{
				newIncomeAnimation.Initialize(this);
			}

			if(_incomeTextIncomeAnimations != null)
			{
				foreach(var incomeAnimation in _incomeTextIncomeAnimations)
				{
					incomeAnimation.Initialize(this);
					incomeAnimation.Clear(true);
				}
			}
		}

		private void HandleDisplayingValueChanged(int newValue)
		{
			var incomeAmount = newValue - _displayedCountTarget;
			_displayedCountTarget = newValue;
			foreach(var newIncomeAnimation in _mainTextIncomeAnimations)
			{
				newIncomeAnimation.Play(false);
			}

			if(_incomeText != null && _incomeTextIncomeAnimations != null && incomeAmount > 0)
			{
				var isShowing = _clearIncomeTimer > 0f;
				foreach(var incomeAnimation in _incomeTextIncomeAnimations)
				{
					if(incomeAnimation.InProgress)
					{
						isShowing = true;
						break;
					}
				}

				if(isShowing)
				{
					_currentShownIncome += incomeAmount;
					_incomeText.text = "+" + _currentShownIncome;

					foreach(var incomeAnimation in _incomeTextIncomeAnimations)
					{
						incomeAnimation.RestartPartially();
					}
				}
				else
				{
					_currentShownIncome = incomeAmount;
					_incomeText.text = "+" + _currentShownIncome;

					foreach(var incomeAnimation in _incomeTextIncomeAnimations)
					{
						incomeAnimation.Play(false);
					}
				}

				_waitingToClearIncome = true;
				_clearIncomeTimer = -1f;
			}
		}

		private void CustomUpdate(float deltaTime)
		{
			_displayedCountFloat = Mathf.Lerp(_displayedCountFloat, _displayedCountTarget, deltaTime * 2.61f);
			var prevCount = _displayedCount;
			_displayedCount = Mathf.RoundToInt(_displayedCountFloat);

			if(prevCount != _displayedCount)
			{
				_text.text = _displayedCount.ToString();
			}

			if(_waitingToClearIncome)
			{
				var anyInProgress = false;

				foreach(var incomeAnimation in _incomeTextIncomeAnimations)
				{
					if(incomeAnimation.InProgress)
					{
						anyInProgress = true;
						break;
					}
				}

				if(!anyInProgress)
				{
					_waitingToClearIncome = false;
					_clearIncomeTimer = _incomeClearDelay;
				}
			}

			if(_clearIncomeTimer > 0f)
			{
				_clearIncomeTimer -= deltaTime;
				if(_clearIncomeTimer <= 0f)
				{
					if(_incomeTextIncomeAnimations != null)
					{
						foreach(var incomeAnimation in _incomeTextIncomeAnimations)
						{
							incomeAnimation.Clear(false);
						}
					}
				}
			}
		}
	}
}
