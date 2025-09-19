#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace game.gameplay_core.characters.ai.utility.editor
{
	public class UtilityWeightHistoryWindow : EditorWindow
	{
		private SubUtilityBase _target;
		private readonly Dictionary<string, List<float>> _weightHistory = new();
		private readonly Dictionary<string, Color> _actionColors = new();
		private Vector2 _scrollPosition;
		private float _timeScale = 1f;
		private int _maxHistoryPoints = 1000;
		private bool _autoScroll = true;
		private float _currentTime;

		private void Initialize()
		{
			if(_target == null)
			{
				return;
			}

			_weightHistory.Clear();
			_actionColors.Clear();

			for(var i = 0; i < _target.Actions.Count; i++)
			{
				var action = _target.Actions[i];
				_weightHistory[action.Id] = new List<float>();
				_actionColors[action.Id] = action.DebugColor;
			}
		}

		private void OnEnable()
		{
			SubUtilityBase.OnWeightUpdate += HandleWeightUpdate;
		}

		private void OnDisable()
		{
			SubUtilityBase.OnWeightUpdate -= HandleWeightUpdate;
		}

		[MenuItem("Tools/AI/Utility Weight History")]
		public static void ShowWindow()
		{
			var window = GetWindow<UtilityWeightHistoryWindow>("Utility Weight History");
			window.Show();
		}

		private void HandleWeightUpdate(object sender, WeightUpdateEventArgs e)
		{
			if(_target == null || sender != _target)
			{
				return;
			}

			_currentTime += e.DeltaTime;

			foreach(var action in e.Actions)
			{
				var actionId = action.Id;
				var weight = action.DebugWeightCache;

				if(!_weightHistory.ContainsKey(actionId))
				{
					_weightHistory[actionId] = new List<float>();
					_actionColors[actionId] = Color.HSVToRGB(Random.value, 0.8f, 1f);
				}

				var history = _weightHistory[actionId];
				history.Add(weight);

				if(history.Count > _maxHistoryPoints)
				{
					history.RemoveAt(0);
				}
			}
		}

		private void OnGUI()
		{
			if(_target == null)
			{
				TryGetTargetFromSelection();
			}

			DrawTargetSelection();

			if(_target == null)
			{
				EditorGUILayout.HelpBox("Select a GameObject with SubUtilityBase component in the hierarchy or assign one manually.", MessageType.Info);
				return;
			}

			DrawControls();
			DrawGraph();
			DrawLegend();

			if(_autoScroll)
			{
				Repaint();
			}
		}

		private void DrawTargetSelection()
		{
			EditorGUILayout.BeginHorizontal();

			var newTarget = (SubUtilityBase)EditorGUILayout.ObjectField("Target", _target, typeof(SubUtilityBase), true);
			if(newTarget != _target)
			{
				_target = newTarget;
				if(_target != null)
				{
					Initialize();
				}
			}

			if(GUILayout.Button("Get from Selection", GUILayout.Width(120)))
			{
				TryGetTargetFromSelection();
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		private void TryGetTargetFromSelection()
		{
			if(Selection.activeGameObject != null)
			{
				var selectedTarget = Selection.activeGameObject.GetComponent<SubUtilityBase>();
				if(selectedTarget != null)
				{
					_target = selectedTarget;
					Initialize();
				}
			}
		}

		private void DrawControls()
		{
			EditorGUILayout.BeginHorizontal();

			if(GUILayout.Button("Clear History"))
			{
				foreach(var history in _weightHistory.Values)
				{
					history.Clear();
				}
				_currentTime = 0f;
			}

			_autoScroll = EditorGUILayout.Toggle("Auto Scroll", _autoScroll);
			_timeScale = EditorGUILayout.Slider("Time Scale", _timeScale, 0.1f, 5f);
			_maxHistoryPoints = EditorGUILayout.IntSlider("Max Points", _maxHistoryPoints, 100, 5000);

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		private void DrawGraph()
		{
			var rect = GUILayoutUtility.GetRect(position.width - 20, 300);
			EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1f));

			if(_weightHistory.Count == 0 || _weightHistory.Values.All(h => h.Count == 0))
			{
				EditorGUI.LabelField(rect, "No data to display", EditorStyles.centeredGreyMiniLabel);
				return;
			}

			var maxWeight = _weightHistory.Values.SelectMany(h => h).DefaultIfEmpty(0).Max();
			var minWeight = _weightHistory.Values.SelectMany(h => h).DefaultIfEmpty(0).Min();
			var weightRange = Mathf.Max(maxWeight - minWeight, 0.1f);

			var maxPoints = _weightHistory.Values.Max(h => h.Count);
			if(maxPoints == 0)
			{
				return;
			}

			Handles.BeginGUI();

			foreach(var kvp in _weightHistory)
			{
				var actionId = kvp.Key;
				var history = kvp.Value;
				var color = _actionColors[actionId];

				if(history.Count < 2)
				{
					continue;
				}

				Handles.color = color;

				for(var i = 1; i < history.Count; i++)
				{
					var prevWeight = history[i - 1];
					var currentWeight = history[i];

					var x1 = rect.x + (float)(i - 1) / (maxPoints - 1) * rect.width;
					var y1 = rect.y + rect.height - (prevWeight - minWeight) / weightRange * rect.height;

					var x2 = rect.x + (float)i / (maxPoints - 1) * rect.width;
					var y2 = rect.y + rect.height - (currentWeight - minWeight) / weightRange * rect.height;

					Handles.DrawLine(new Vector3(x1, y1), new Vector3(x2, y2));
				}
			}

			Handles.color = Color.gray;
			Handles.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.y + rect.height));
			Handles.DrawLine(new Vector3(rect.x, rect.y + rect.height), new Vector3(rect.x + rect.width, rect.y + rect.height));

			Handles.EndGUI();

			var labelRect = new Rect(rect.x + 5, rect.y + 5, 100, 20);
			EditorGUI.LabelField(labelRect, $"Max: {maxWeight:F2}", EditorStyles.miniLabel);
			labelRect.y = rect.y + rect.height - 20;
			EditorGUI.LabelField(labelRect, $"Min: {minWeight:F2}", EditorStyles.miniLabel);
		}

		private void DrawLegend()
		{
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Legend:", EditorStyles.boldLabel);

			foreach(var kvp in _actionColors)
			{
				EditorGUILayout.BeginHorizontal();

				var colorRect = GUILayoutUtility.GetRect(20, 20);
				EditorGUI.DrawRect(colorRect, kvp.Value);

				var currentWeight = _weightHistory.ContainsKey(kvp.Key) && _weightHistory[kvp.Key].Count > 0
					? _weightHistory[kvp.Key].Last()
					: 0f;

				EditorGUILayout.LabelField($"{kvp.Key}: {currentWeight:F2}");

				EditorGUILayout.EndHorizontal();
			}
		}
	}
}
#endif
