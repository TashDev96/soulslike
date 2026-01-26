using Cysharp.Threading.Tasks;
using dream_lib.src.utils.components;
using game.input;
using UnityEngine;

namespace game.ui
{
	public class UiDomain
	{
		private MainCanvasInstaller _mainCanvasInstaller;

		private UiLocationHUD _locationHud;
		private UnityEventsListener _eventsListener;

		public bool IsInventoryOpen => _mainCanvasInstaller.Inventory.gameObject.activeSelf;

		public async UniTask Initialize()
		{
			var mainCanvasPrefab = await AddressableManager.LoadAssetAsync<GameObject>(AddressableAssetNames.MainCanvas, AssetOwner.Game);

			_mainCanvasInstaller = Object.Instantiate(mainCanvasPrefab).GetComponent<MainCanvasInstaller>();
			_mainCanvasInstaller.Inventory.gameObject.SetActive(false);
			Object.DontDestroyOnLoad(_mainCanvasInstaller.gameObject);

			_locationHud = _mainCanvasInstaller.UiLocationHUD;

			GameStaticContext.Instance.WorldToScreenUiParent.Value = _mainCanvasInstaller.WorldToScreenRoot;

			_eventsListener = UnityEventsListener.Create("_UiDomainEventsListener");
			_eventsListener.OnUpdate += CustomUpdate;
		}

		public void ShowLocationUi(UiLocationHUD.Context context)
		{
			_locationHud.SetContext(context);
		}

		public void ToggleInventoryScreen()
		{
			_mainCanvasInstaller.Inventory.ToggleIsShowing();
		}

		private void CustomUpdate()
		{
			if(InputAdapter.GetButtonDown(InputAxesNames.Inventory))
			{
				ToggleInventoryScreen();
			}
		}
	}
}
