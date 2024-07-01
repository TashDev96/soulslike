using dream_lib.src.extensions;
using dream_lib.src.utils.serialization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace game.gameplay_core
{
	public abstract class SavableSceneObject<T> : SavableSceneObjectAbstract, IOnSceneUniqueIdOwner, ISavable
	{

		protected T Data { get; set; }

		public abstract void InitializeFirstTime();

		public override string Serialize()
		{
			return JsonUtility.ToJson(Data, true);
		}

		public override void Deserialize(string data)
		{
			Data = JsonUtility.FromJson<T>(data);
			OnDeserialize();
		}

		[Button]
		public override void GenerateUniqueId()
		{
			UniqueId = transform.GetFullPathInScene() + Random.value;
		}
	}
}
