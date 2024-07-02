namespace game.gameplay_core
{
	public class SpawnedObjectController
	{
		public SpawnedObjectSaveData SaveData { get; private set; }
		public SceneSavableObjectBase SceneInstance;

		public void LoadSave(SpawnedObjectSaveData data)
		{
			SaveData = data;
			SceneInstance.LoadSave(SaveData.ObjectSaveData);
		}
	}
}
