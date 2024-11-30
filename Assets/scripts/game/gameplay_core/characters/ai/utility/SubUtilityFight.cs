using dream_lib.src.utils.data_types;
using game.gameplay_core.characters.ai.editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace game.gameplay_core.characters.ai
{
	[OdinDontRegister]
	public class SubUtilityFight : MonoBehaviour, ISerializationCallbackReceiver
	{
		public SerializableDictionary<string, GoalsChain> GoalChains;
		public SerializableDictionary<string, UtilityAction> Actions;

		//chain of goals
		//chain examples:
		//multiple attacks
		//jump back then heal
		//jump attack then roll back

		//input: enemy
		//input: self stats
		//input: inventory

		//input: fight history data

		//list of attacks
		//list of defences
		//list of movement
		//list of stupidities

		public void HandleCharacterUpdate(float deltaTime)
		{
		}

		public void OnBeforeSerialize()
		{
#if UNITY_EDITOR
			UtilityEditorHelper.CurrentContextActions = Actions;
#endif
		}

		public void OnAfterDeserialize()
		{
		}
	}
}
