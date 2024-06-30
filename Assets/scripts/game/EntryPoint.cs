using gameplay_meta;
using UnityEngine;

namespace application
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
