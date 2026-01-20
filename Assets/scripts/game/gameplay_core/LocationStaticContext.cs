using System.Collections.Generic;
using dream_lib.src.reactive;
using game.gameplay_core.camera;
using game.gameplay_core.characters;
using game.gameplay_core.location.location_save_system;

namespace game.gameplay_core
{
	public class LocationStaticContext
	{
		public static LocationStaticContext Instance { get; set; }

		public LocationSaveData LocationSaveData { get; set; }

		public CharacterDomain Player { get; set; }

		public SceneSavableObjectBase[] SceneSavableObjects { get; set; }

		public List<CharacterDomain> Characters { get; set; }
		public List<SpawnedObjectController> SpawnedObjects { get; set; } = new();
		public ReactiveCommand<float> LocationUpdate { get; set; }
		public ReactiveCommand<float> LocationUiUpdate { get; set; }
		public ReactiveProperty<float> LocationTime { get; set; }
		public ICameraController CameraController { get; set; }
	}
}
