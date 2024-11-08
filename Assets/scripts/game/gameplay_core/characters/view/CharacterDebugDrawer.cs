using System;
using System.Diagnostics;
using dream_lib.src.extensions;
using dream_lib.src.utils.drawers;
using game.gameplay_core.characters.ai;
using game.gameplay_core.characters.commands;
using game.gameplay_core.characters.state_machine;
using game.gameplay_core.characters.state_machine.states.attack;
using game.gameplay_core.damage_system;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters.view
{
	[Serializable]
	public class CharacterDebugDrawer
	{
		public bool DrawStateMachineInfo;
		public bool DrawBrainInfo;

		private bool _initialized;
		private CharacterContext _context;
		private ICharacterBrain _brain;
		private Transform _transform;
		private CharacterStateMachine _stateMachine;
		private GUIStyle _textStyle;

		private GizmoGraphDrawer _graphDrawer;
		private int _attackIndex;
		private bool _comboTriggered;

		private float AttackGraphY => _attackIndex / 10f;

		[Conditional("UNITY_EDITOR")]
		public void Initialize(Transform transform, CharacterContext context, CharacterStateMachine stateMachine, ICharacterBrain brain)
		{
			_transform = transform;
			_context = context;
			_stateMachine = stateMachine;
			_brain = brain;
			_initialized = true;

			_textStyle = new GUIStyle
			{
				normal = new GUIStyleState { textColor = Color.white },
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.LowerLeft
			};
			_graphDrawer = new GizmoGraphDrawer();
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
			if(DrawBrainInfo)
			{
				str += _brain.GetDebugSting();
			}

			Handles.Label(_transform.position + Vector3.up * 3f, str, _textStyle);

			_graphDrawer.Draw(_transform.position + Vector3.up * (3f + 2 * HandleUtility.GetHandleSize(_transform.position)));

			if(_stateMachine.CurrentState.Value is AttackState attackState)
			{
				if(_context.InputData.Command == CharacterCommand.Attack)
				{
					_graphDrawer.FreePoints.Add(new GraphPoint(attackState.Time, AttackGraphY)
					{
						Color = Color.blue,
						Size = 0.03f
					});
				}
			}
		}

#endif

		[Conditional("UNITY_EDITOR")]
		public void AddAttackGraph(AttackConfig currentAttackConfig)
		{
			_attackIndex++;
			_comboTriggered = false;
			var line = _graphDrawer.AddLine(_attackIndex.ToString());
			line.AddRange(new[]
			{
				new GraphPoint(0, AttackGraphY),
				new GraphPoint(currentAttackConfig.ExitToComboTime.x * currentAttackConfig.Duration, AttackGraphY)
				{
					Color = Color.green
				},
				new GraphPoint(currentAttackConfig.ExitToComboTime.y * currentAttackConfig.Duration, AttackGraphY),
				new GraphPoint(currentAttackConfig.Duration, AttackGraphY)
			});
		}

		[Conditional("UNITY_EDITOR")]
		public void AddAttackComboAttempt(float time)
		{
			if(!_comboTriggered)
			{
				_comboTriggered = true;
				_graphDrawer.FreePoints.Add(new GraphPoint(time, AttackGraphY)
				{
					Color = Color.red,
					Size = 0.05f
				});
			}
		}
	}
}
