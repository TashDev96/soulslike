using UnityEngine;

namespace game.gameplay_core.camera
{
	public class CameraSettings : MonoBehaviour
	{
		[SerializeField]
		private float _cameraAltitude;

		public float CameraAltitude => _cameraAltitude;
	}
}
