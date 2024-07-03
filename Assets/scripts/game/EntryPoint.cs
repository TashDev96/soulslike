using application;
using UnityEngine;

namespace game
{
	public class EntryPoint : MonoBehaviour
	{
		[SerializeField]
		private bool _sceneDebugMode;

		private void Awake()
		{
			if(ApplicationDomain.Initialized)
			{
				return;
			}
			ApplicationDomain.Initialize(new GameDomain(_sceneDebugMode));
		}
	}
}
