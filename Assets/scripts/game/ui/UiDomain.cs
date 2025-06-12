using Cysharp.Threading.Tasks;
using UnityEngine;

namespace game.ui
{
	public class UiDomain
	{
		private MainCanvasInstaller _mainCanvasInstaller;

		private UiLocationHUD _locationHud;

		public async UniTask Initialize()
		{
			var mainCanvasPrefab = await AddressableManager.LoadAssetAsync<GameObject>(AddressableAssetNames.MainCanvas, AssetOwner.Game);

			_mainCanvasInstaller = Object.Instantiate(mainCanvasPrefab).GetComponent<MainCanvasInstaller>();
			Object.DontDestroyOnLoad(_mainCanvasInstaller.gameObject);

			_locationHud = _mainCanvasInstaller.UiLocationHUD;

			GameStaticContext.Instance.WorldToScreenUiParent.Value = _mainCanvasInstaller.WorldToScreenRoot;
		}

		public void ShowLocationUi(UiLocationHUD.Context context)
		{
			_locationHud.SetContext(context);
		}
	}
}
