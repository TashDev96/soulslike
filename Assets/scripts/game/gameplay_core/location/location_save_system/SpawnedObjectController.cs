namespace game.gameplay_core.location.location_save_system
{
	public class SpawnedObjectController
	{
		public SceneSavableObjectBase SceneInstance;
		public SpawnedObjectSaveData SaveData { get; private set; }

		public void LoadSave(SpawnedObjectSaveData data)
		{
			SaveData = data;
			SceneInstance.LoadSave(SaveData.ObjectSaveData);
		}
	}
}
