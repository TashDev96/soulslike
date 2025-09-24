using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace game.gameplay_core.characters.ai.utility.considerations.utils
{
	[Serializable]
	public class PerlinConfig
	{
		[SerializeField]
		private Octave[] _octaves;

		[Range(0f, 60f)]
		private float _previewStartTime = 0f;
		private float _previewDuration = 10f;

		[SerializeField, Range(0f, 1f)]
		private float _threshold = 0.5f;
		
		public float Threshold => _threshold;

		private string _simulationResults = "";

		public float Evaluate(float time)
		{
			if(_octaves == null || _octaves.Length == 0)
			{
				return Mathf.PerlinNoise1D(time);
			}

			var result = 0f;
			var totalAmplitude = 0f;

			for(var i = 0; i < _octaves.Length; i++)
			{
				var octave = _octaves[i];
				if(octave.Disabled)
				{
					continue;
				}
				var sampleTime = (time + octave.TimeOffset) * octave.TimeScale;
				var noiseValue = Mathf.PerlinNoise1D(sampleTime);

				if(octave.AddMode || i == 0)
				{
					result += noiseValue * octave.AmplitudeMultiplier;
				}
				else
				{
					result = Mathf.Lerp(result, noiseValue, octave.AmplitudeMultiplier);
				}

				totalAmplitude += octave.AmplitudeMultiplier;
			}

			return totalAmplitude > 0f ? result / totalAmplitude : result;
		}

		[Button("Simulate 1 Hour")]
		private void SimulateOneHour()
		{
			const float simulationDuration = 3600f;
			const float step = 0.016f;
			const int samples = (int)(simulationDuration / step);

			var aboveLineDurations = new List<float>();
			var currentAboveDuration = 0f;
			var totalTimeAbove = 0f;
			var wasAbove = false;

			for(var i = 0; i < samples; i++)
			{
				var time = i * step;
				var value = Evaluate(time);
				var isAbove = value > _threshold;

				if(isAbove)
				{
					totalTimeAbove += step;
					currentAboveDuration += step;
					wasAbove = true;
				}
				else
				{
					if(wasAbove && currentAboveDuration > 0f)
					{
						aboveLineDurations.Add(currentAboveDuration);
						currentAboveDuration = 0f;
					}
					wasAbove = false;
				}
			}

			if(wasAbove && currentAboveDuration > 0f)
			{
				aboveLineDurations.Add(currentAboveDuration);
			}

			var percentageAbove = (totalTimeAbove / simulationDuration) * 100f;
			var maxDuration = aboveLineDurations.Count > 0 ? aboveLineDurations.Max() : 0f;
			var medianDuration = 0f;

			if(aboveLineDurations.Count > 0)
			{
				var sortedDurations = aboveLineDurations.OrderBy(x => x).ToArray();
				var middleIndex = sortedDurations.Length / 2;
				medianDuration = sortedDurations.Length % 2 == 0
					? (sortedDurations[middleIndex - 1] + sortedDurations[middleIndex]) / 2f
					: sortedDurations[middleIndex];
			}

			_simulationResults = $"1 Hour Simulation Results:\n" +
			                    $"Time above line: {percentageAbove:F1}%\n" +
			                    $"Max duration above: {maxDuration:F1}s\n" +
			                    $"Median duration above: {medianDuration:F1}s\n" +
			                    $"Episodes above line: {aboveLineDurations.Count}";
		}

		[OnInspectorGUI]
		private void DrawPerlinPreview()
		{
#if UNITY_EDITOR
			

			GUILayout.Space(5);

			const float step = 0.016f;
			var samples = (int)(_previewDuration / step);
			const float graphHeight = 100f;
			var graphWidth = EditorGUIUtility.currentViewWidth - 240f;

			var rect = GUILayoutUtility.GetRect(graphWidth, graphHeight);
			EditorGUI.DrawRect(rect, Color.black);

			var currentEvent = Event.current;
			if(currentEvent.type == EventType.MouseDown && currentEvent.button == 0 && rect.Contains(currentEvent.mousePosition))
			{
				var relativeY = (currentEvent.mousePosition.y - rect.y) / graphHeight;
				_threshold = 1f - relativeY;
				_threshold = Mathf.Clamp01(_threshold);
				currentEvent.Use();
			}

			var points = new Vector3[samples];
			const float minValue = 0f;
			const float maxValue = 1f;
			const float range = maxValue - minValue;

			for(var i = 0; i < samples; i++)
			{
				var time = _previewStartTime + i * step;
				var value = Evaluate(time);
				points[i] = new Vector3(i * step, value, 0);
			}

			Handles.BeginGUI();

			Handles.color = Color.green;
			for(var i = 0; i < samples - 1; i++)
			{
				var x1 = rect.x + points[i].x / _previewDuration * graphWidth;
				var y1 = rect.y + (1f - (points[i].y - minValue) / range) * graphHeight;
				var x2 = rect.x + points[i + 1].x / _previewDuration * graphWidth;
				var y2 = rect.y + (1f - (points[i + 1].y - minValue) / range) * graphHeight;

				Handles.DrawLine(new Vector3(x1, y1, 0), new Vector3(x2, y2, 0));
			}

			Handles.color = Color.red;
			var altitudeY = rect.y + (1f - _threshold) * graphHeight;
			Handles.DrawLine(new Vector3(rect.x, altitudeY, 0), new Vector3(rect.x + graphWidth, altitudeY, 0));

			Handles.EndGUI();

			GUILayout.Space(10);
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Start Time:", GUILayout.Width(80));
			_previewStartTime = EditorGUILayout.Slider(_previewStartTime, 0f, 60f);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Duration:", GUILayout.Width(80));
			_previewDuration = EditorGUILayout.FloatField(_previewDuration);
			_previewDuration = Mathf.Max(0.1f, _previewDuration);
			GUILayout.EndHorizontal();

			if(!string.IsNullOrEmpty(_simulationResults))
			{
				GUILayout.Space(10);
				var style = new GUIStyle(GUI.skin.box)
				{
					alignment = TextAnchor.UpperLeft,
					wordWrap = true,
					padding = new RectOffset(10, 10, 10, 10)
				};
				GUILayout.Label(_simulationResults, style);
			}
			
#endif
		}

		[Serializable]
		private struct Octave
		{
			public bool Disabled;
			public float TimeOffset;
			public float TimeScale;
			public float AmplitudeMultiplier;
			public bool AddMode;
		}
	}
}
