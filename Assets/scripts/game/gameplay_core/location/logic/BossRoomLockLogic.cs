using System.Collections;
using game.gameplay_core.characters;
using UnityEngine;

namespace game.gameplay_core.location.logic
{
	public class BossRoomLockLogic : MonoBehaviour
	{
		[SerializeField]
		private CharacterDomain _boss;

		[SerializeField]
		private GameObject _wallsRoot;

		private bool _battleStarted;

		private void OnEnable()
		{
			_wallsRoot.SetActive(false);
			StartCoroutine(CustomUpdate());
		}

		private IEnumerator CustomUpdate()
		{
			while(true)
			{
				if(LocationStaticContext.Instance == null)
				{
					yield return null;
					continue;
				}

				if(!_battleStarted)
				{
					if(LocationStaticContext.Instance.CurrentlyFightingBoss.Value == _boss)
					{
						_wallsRoot.SetActive(true);
						_battleStarted = true;
					}
				}
				else
				{
					if(_boss.Context.IsDead.Value)
					{
						_wallsRoot.SetActive(false);
						yield break;
					}
				}
				yield return null;
			}
		}
	}
}
