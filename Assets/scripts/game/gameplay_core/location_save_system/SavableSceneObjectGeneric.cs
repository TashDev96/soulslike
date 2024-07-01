namespace game.gameplay_core
{
	public abstract class SavableSceneObjectGeneric<T> : SceneSavableObjectBase where T : BaseSaveData
	{
		protected T SaveData { get; private set; }

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
