using System.Collections.Generic;
using DG.Tweening;
using dream_lib.src.extensions;
using game.gameplay_core.characters.logic;
using game.gameplay_core.location;
using UnityEngine;

namespace game.gameplay_core.ui.screenspace.flight
{
	public class FlightUiView : MonoBehaviour
	{
		[SerializeField]
		private CanvasGroup _canvasGroup;
		
		[SerializeField]
		private GameObject _flapItemPrefab;
		private FlyingLogic _logic;

		private List<FlapItemView> _itemViews = new();

		public void Initialize()
		{
			var context = LocationStaticContext.Instance.Player.Context;
			_logic = context.Logic.FlyingLogic;

			transform.DestroyAllChildren();
			
			context.IsBirdMode.OnChanged += HandleBirdModeChanged;

			_logic.MaxFlapsCount.OnChanged += HandleMaxFlapsChanged;
			_logic.FlapsLeftCount.OnChangedFromTo += HandleFlapsCountChanged;
			_logic.CurrentFeatherGlideTimeLeft.OnChanged += HandleGlideTimeChanged;
			
			HandleMaxFlapsChanged(_logic.MaxFlapsCount.Value);
			HandleBirdModeChanged(context.IsBirdMode.Value);
			HandleFlapsCountChanged(0, _logic.FlapsLeftCount.Value);
		}

		private void HandleBirdModeChanged(bool isFlyingMode)
		{
			_canvasGroup.DOKill();
			if(isFlyingMode)
			{
				_canvasGroup.DOFade(1f, 0.5f);
			}
			else
			{
				_canvasGroup.DOFade(0f, 0.5f).SetDelay(3f);
			}
		}

		private void HandleGlideTimeChanged(float timeLeft)
		{
			var context = LocationStaticContext.Instance.Player.Context;
			var maxTime = context.Config.Flying.GlideTimePerFeather;
			var fill = maxTime > 0 ? Mathf.Clamp01(timeLeft / maxTime) : 0f;
			
			var currentIndex = _logic.FlapsLeftCount.Value - 1;
			if(currentIndex >= 0 && currentIndex < _itemViews.Count)
			{
				_itemViews[currentIndex].SetFillAmount(fill);
			}
		}

		private void HandleFlapsCountChanged(int from, int to)
		{
			for(var i = 0; i < _itemViews.Count; i++)
			{
				_itemViews[i].SetIsEmpty(i >= to);
				if(i < to - 1)
				{
					_itemViews[i].SetFillAmount(1f);
				}
				else if(i >= to)
				{
					_itemViews[i].SetFillAmount(0f);
				}
			}
			HandleGlideTimeChanged(_logic.CurrentFeatherGlideTimeLeft.Value);
		}

		private void HandleMaxFlapsChanged(int count)
		{
			while(_itemViews.Count < count)
			{
				var newItem = Instantiate(_flapItemPrefab, transform);
				_itemViews.Add(newItem.GetComponent<FlapItemView>());
			}

			for(var i = 0; i < _itemViews.Count; i++)
			{
				_itemViews[i].gameObject.SetActive(i < count);
			}
		}
	}
}
