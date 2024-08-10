using System;
using dream_lib.src.extensions;
using game.gameplay_core.characters.state_machine;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters.view
{
	[Serializable]
	public struct CharacterDebugDrawer
	{
		public bool DrawStateMachineInfo;
		private bool _initialized;
		private CharacterContext _context;
		private Transform _transform;
		private CharacterStateMachine _stateMachine;
		private GUIStyle _textStyle;

		public void Initialize(Transform transform, CharacterContext context, CharacterStateMachine stateMachine)
		{
			_transform = transform;
			_context = context;
			_stateMachine = stateMachine;
			_initialized = true;

			_textStyle = new GUIStyle
			{
				normal = new GUIStyleState { textColor = Color.white },
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.LowerLeft
			};
		}

#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			if(!_initialized)
			{
				return;
			}

			var str = "";

			str += $"hp: {_context.CharacterStats.Hp.Value.CeilFormat(1)}/{_context.CharacterStats.HpMax.Value.CeilFormat()}\n";

			if(DrawStateMachineInfo)
			{
				str += _stateMachine.GetDebugString();
			}

			Handles.Label(_transform.position + Vector3.up * 3f, str, _textStyle);
		}

#endif
	}
}
