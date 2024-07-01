using dream_lib.src.utils.serialization;
using UnityEngine;

namespace game.gameplay_core
{
	public abstract class SavableSceneObjectAbstract : MonoBehaviour, IOnSceneUniqueIdOwner, ISavable
	{
		[field: SerializeField]
		public string UniqueId { get; protected set; }

		public abstract string Serialize();

		public abstract void Deserialize(string data);
		public abstract void OnDeserialize();

		public abstract void GenerateUniqueId();
	}
}
