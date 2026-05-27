using Cysharp.Threading.Tasks;
using game.gameplay_core.location;
using UnityEngine;

namespace game
{
	public class ViewsOrchestra
	{
		public static async UniTask ShowSoftCurrencyDrop(Vector3 spawnWorldPos, int counterDelayId)
		{

			await LocationStaticContext.Instance.PlayerVfxView.ShowExperienceFlight(spawnWorldPos);

			if(LocationStaticContext.Instance.UnloadCancellationTokenSource.IsCancellationRequested)
			{
				return;
			}
			GameStaticContext.Instance.PlayerSave.SoftCurrency.ResolveDelay(counterDelayId);
		}
	}
}
