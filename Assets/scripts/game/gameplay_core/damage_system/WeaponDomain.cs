using UnityEngine;

namespace game.gameplay_core.damage_system
{
	public class WeaponDomain : MonoBehaviour
	{
		public WeaponConfig Config;

		public void DrawDebugCast(bool attack)
		{
			Debug.DrawLine(transform.TransformPoint(Vector3.zero), transform.TransformPoint(Vector3.up*2), attack?Color.red:Color.green,5f);
		}
	}
}
