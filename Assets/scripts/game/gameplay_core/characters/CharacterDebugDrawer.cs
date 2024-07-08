using System;
using game.gameplay_core.characters.state_machine;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters
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
		
		#if UNITY_EDITOR
		public void OnDrawGizmos()
		{
			if(!_initialized)
			{
				return;
			}

			var str = "";

			if(DrawStateMachineInfo)
			{
				str += $"state:   {_stateMachine.CurrentState.GetType().Name}\n";
				str += $"command: {_context.InputData.Command}\n";
			}

			Handles.Label(_transform.position + Vector3.up * 3f, str, _textStyle);
		}
		
		#endif

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
	}
}
