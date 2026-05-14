using System;
using dream_lib.src.extensions;
using game.gameplay_core.characters.stats.config;
using game.gameplay_core.location;
using UnityEngine;

namespace game.ui.stats
{
	public class StatsScreenView : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _baseStatsContainer;

		[SerializeField]
		private RectTransform _resultStatsContainer;

		[SerializeField]
		private GameObject _statViewPrefab;

		public void Show()
		{
			var player = LocationStaticContext.Instance.Player.Context;

			_baseStatsContainer.DestroyAllChildren();
			_resultStatsContainer.DestroyAllChildren();

			foreach(var kvp in player.CharacterStats.AllStats)
			{
				if(kvp.Value.IsHidden)
				{
					continue;
				}

				var statView = Instantiate(_statViewPrefab).GetComponent<StatView>();
				statView.Initialize(kvp.Value);

				switch(kvp.Value.Config.Type)
				{
					case StatType.Base:
						statView.transform.SetParent(_baseStatsContainer);
						break;
					case StatType.Result:
						statView.transform.SetParent(_resultStatsContainer);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			gameObject.SetActive(true);
		}

		public void Toggle()
		{
			if(!gameObject.activeSelf)
			{
				Show();
			}
			else
			{
				gameObject.SetActive(false);
			}
		}
	}
}
