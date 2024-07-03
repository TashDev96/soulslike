using dream_lib.src.extensions;
using dream_lib.src.utils.serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core.location_save_system
{
	public abstract class SceneSavableObjectBase : MonoBehaviour, IOnSceneUniqueIdOwner
	{
		[field: SerializeField]
		public string UniqueId { get; private set; }
		public abstract void InitializeFirstTime();

		public abstract void LoadSave(BaseSaveData data);
		public abstract BaseSaveData GetSave();

		[Button]
		public void GenerateUniqueId()
		{
			UniqueId = transform.GetFullPathInScene() + Random.value;
		}
	}
}
