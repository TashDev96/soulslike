using Cysharp.Threading.Tasks;
using UnityEngine;

namespace game.ui
{
	public class UiDomain
	{
		private MainCanvasInstaller _mainCanvasInstaller;

		public async UniTask Initialize()
		{
			var mainCanvasPrefab = await AddressableManager.LoadAssetAsync<GameObject>(AddressableAssetNames.MainCanvas, AssetOwner.Game);

			_mainCanvasInstaller = Object.Instantiate(mainCanvasPrefab).GetComponent<MainCanvasInstaller>();
			Object.DontDestroyOnLoad(_mainCanvasInstaller.gameObject);
		}
	}
}
