using dream_lib.src.reactive;
using game.gameplay_core.camera;
using UnityEngine;

namespace game.gameplay_core.worldspace_ui
{
	public class FloatingTextsManager
	{
		private const string FloatingTextViewAddress = "FloatingTextView";

		private readonly ICameraController _cameraController;
		private readonly ReactiveCommand<float> _locationUpdate;

		public FloatingTextsManager(ICameraController cameraController, ReactiveCommand<float> locationUpdate)
		{
			_cameraController = cameraController;
			_locationUpdate = locationUpdate;
		}

		public void ShowFloatingText(string text, FloatingTextView.TextColorVariant color, Vector3 worldPosition)
		{
			var prefab = AddressableManager.GetPreloadedAsset<GameObject>(FloatingTextViewAddress);
			var instance = Object.Instantiate(prefab);
			var view = instance.GetComponent<FloatingTextView>();

			view.Initialize(text, color, worldPosition, new FloatingTextView.Context
			{
				CameraController = _cameraController,
				LocationUpdate = _locationUpdate
			});
		}
	}
}
