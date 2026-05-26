using dream_lib.src.reactive;
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

		[SerializeReference]
		private UiAnimationBase[] _newIncomeAnimations;

		private int _displayedCount;
		private float _displayedCountFloat;
		private int _displayedCountTarget;

		private ReactivePropertyWithDelayedDisplayInt ValueSource => GameStaticContext.Instance.PlayerSave.SoftCurrency;

		public void Initialize()
		{
			_displayedCount = ValueSource.RealValue;
			_displayedCountFloat = _displayedCount;
			_displayedCountTarget = _displayedCount;
			_text.text = _displayedCount.ToString();

			ValueSource.OnDisplayingValueChanged += HandleDisplayingValueChanged;
			LocationStaticContext.Instance.LocationUiUpdate.OnExecute += CustomUpdate;

			foreach(var newIncomeAnimation in _newIncomeAnimations)
			{
				newIncomeAnimation.Initialize(this);
			}
		}

		private void HandleDisplayingValueChanged(int newValue)
		{
			_displayedCountTarget = newValue;
			foreach(var newIncomeAnimation in _newIncomeAnimations)
			{
				newIncomeAnimation.Play(false);
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
		}
	}
}
