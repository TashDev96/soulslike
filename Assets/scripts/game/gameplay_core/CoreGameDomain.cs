using Cysharp.Threading.Tasks;
using UnityEngine;

namespace game.gameplay_core
{
	public class CoreGameDomain
	{
		private LocationDomain _locationDomain;

		private bool _initialized = false;

		public async UniTask Initialize()
		{
			await PreloadCoreGameAssets();

			_locationDomain = new LocationDomain();
			_locationDomain.Initialize();
		}

		public void PlayOnDebugLocation()
		{
			Initialize().Forget();
		}

		public void PlayOnLocation()
		{
			//load scene
		}

		private async UniTask PreloadCoreGameAssets()
		{
			await AddressableManager.LoadAssetAsync<GameObject>(AddressableAssetNames.Player, AssetOwner.Game);
		}
	}
}
