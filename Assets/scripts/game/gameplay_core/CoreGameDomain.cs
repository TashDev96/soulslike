using Cysharp.Threading.Tasks;
using UnityEngine;

namespace game.gameplay_core
{
	public class CoreGameDomain
	{
		private LocationDomain _locationDomain;

		private bool _initialized = false;

		private async UniTask Initialize()
		{
			await PreloadCoreGameAssets();

			_locationDomain = new LocationDomain();
			_locationDomain.Initialize();
		}

		public async UniTask PlayOnDebugLocation()
		{
			await Initialize();
		}

		public async UniTask PlayOnLocation()
		{
			//load scene
		}

		private async UniTask PreloadCoreGameAssets()
		{
			await AddressableManager.LoadAssetAsync<GameObject>(AddressableAssetNames.Player, AssetOwner.Game);
			await AddressableManager.LoadAssetAsync<GameObject>(AddressableAssetNames.CharacterUi, AssetOwner.Game);
		}
	}
}
