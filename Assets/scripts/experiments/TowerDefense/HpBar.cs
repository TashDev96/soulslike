using UnityEngine;
using UnityEngine.UI;

namespace TowerDefense
{
	public class HpBar : MonoBehaviour
	{
		[SerializeField] private Image fillImage;

		private ZombieUnit trackedZombie;
		private Camera playerCamera;
		private Canvas canvas;
		private Vector3 worldOffset = new Vector3(0, 2f, 0);

		public bool IsActive { get; private set; }

		public void Initialize(Camera camera, Canvas parentCanvas)
		{
			playerCamera = camera;
			canvas = parentCanvas;
		}

		public void AttachToZombie(ZombieUnit zombie)
		{
			trackedZombie = zombie;
			IsActive = true;
			gameObject.SetActive(true);
			UpdateHpBar();
		}

		public void Detach()
		{
			trackedZombie = null;
			IsActive = false;
			gameObject.SetActive(false);
		}

		public ZombieUnit GetTrackedZombie()
		{
			return trackedZombie;
		}

		private void LateUpdate()
		{
			if (!IsActive || trackedZombie == null || trackedZombie.IsDead)
			{
				return;
			}

			UpdateHpBar();
			UpdatePosition();
		}

		private void UpdateHpBar()
		{
			if (fillImage != null && trackedZombie != null)
			{
				float healthPercentage = trackedZombie.GetHealthPercentage();
				fillImage.transform.localScale = new Vector3(healthPercentage, 1f, 1f);
			}
		}

		private void UpdatePosition()
		{
			if (trackedZombie != null && playerCamera != null)
			{
				Vector3 worldPosition = trackedZombie.Position + worldOffset;
				Vector3 screenPosition = playerCamera.WorldToScreenPoint(worldPosition);

				if (screenPosition.z > 0)
				{
					transform.position = screenPosition;
				}
			}
		}
	}
}

