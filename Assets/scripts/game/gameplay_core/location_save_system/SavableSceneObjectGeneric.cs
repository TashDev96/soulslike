namespace game.gameplay_core.location_save_system
{
	public abstract class SavableSceneObjectGeneric<T> : SceneSavableObjectBase where T : BaseSaveData
	{
		protected T SaveData { get; set; }

		protected abstract void InitializeAfterSaveLoaded();

		public override void LoadSave(BaseSaveData data)
		{
			SaveData = (T)data;
			InitializeAfterSaveLoaded();
		}

		public override BaseSaveData GetSave()
		{
			return SaveData;
		}
	}
}
