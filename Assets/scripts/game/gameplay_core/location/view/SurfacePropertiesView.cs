using UnityEngine;

namespace game.gameplay_core.location.view
{
	public class SurfacePropertiesView : MonoBehaviour
	{
		[field: SerializeField]
		public bool StopWeapons { get; private set; }
	}
}
