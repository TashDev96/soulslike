using Cysharp.Threading.Tasks;
using game.gameplay_core.location;
using UnityEngine;

namespace game
{
	public class ViewsOrchestra
	{
		public static void ShowSoftCurrencyDrop(Vector3 spawnWorldPos, int counterDelayId)
		{
			var vfxDuration = 2000;

			//show vfx

			//resolve delay

			Loop().Forget();

			async UniTask Loop()
			{
				await UniTask.Delay(vfxDuration); //, cancellationToken: LocationStaticContext.Instance.UnloadCancellationTokenSource.Token);
				if(LocationStaticContext.Instance.UnloadCancellationTokenSource.IsCancellationRequested)
				{
					return;
				}

				GameStaticContext.Instance.PlayerSave.SoftCurrency.ResolveDelay(counterDelayId);
			}
		}
	}
}
