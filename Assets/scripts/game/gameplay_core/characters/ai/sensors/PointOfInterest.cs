using UnityEngine;

namespace game.gameplay_core.characters.ai.sensors
{
	public class PointOfInterest
	{
		public enum Type
		{
			Enemy,
			Ally,
			Item,
			Mechanism
		}

		public GameObject sas;
		public Vector3 position;

		public float Time;
		public float TimePassed => UnityEngine.Time.time - Time;
	}
}
