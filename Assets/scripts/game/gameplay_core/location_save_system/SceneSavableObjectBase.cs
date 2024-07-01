using dream_lib.src.extensions;
using dream_lib.src.utils.serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core
{
	public abstract class SceneSavableObjectBase : MonoBehaviour, IOnSceneUniqueIdOwner
	{
		[field: SerializeField]
		public string UniqueId { get; private set; }

		public abstract void LoadSave(BaseSaveData data);
		public abstract BaseSaveData GetSave();
		public abstract void InitializeFirstTime();
		
		[Button]
		public void GenerateUniqueId()
		{
			UniqueId = transform.GetFullPathInScene() + Random.value;
		}
	}
}
