namespace game.gameplay_core.location.location_save_system
{
	public abstract class SavableSceneObjectGeneric<T> : SceneSavableObjectBase where T : BaseSaveData
	{
		protected T Data { get; set; }

		protected abstract void InitializeAfterSaveLoaded();

		public override void LoadSave(BaseSaveData data)
		{
			Data = (T)data;
			InitializeAfterSaveLoaded();
		}

		public override BaseSaveData GetSaveData()
		{
			return Data;
		}
	}
}
