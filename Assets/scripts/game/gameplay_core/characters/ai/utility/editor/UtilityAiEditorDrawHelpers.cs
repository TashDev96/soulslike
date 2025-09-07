using game.gameplay_core.characters.ai.utility.considerations.value_sources;
using game.gameplay_core.characters.ai.utility.considerations.weights;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters.ai.utility.editor
{
	public static class UtilityAiEditorDrawHelpers
	{
		public static void DrawUtilityWeightCompareVisualization(ValueSourceRandom source, UtilityWeightCompare weightCompare)
		{
			var curve = source.RandomValuesOverTime;
			if(curve == null || curve.length == 0)
			{
				return;
			}

			var rect = EditorGUILayout.GetControlRect(false, 120);
			var padding = 10f;
			var drawRect = new Rect(rect.x + padding, rect.y + padding, rect.width - padding * 2, rect.height - padding * 2);

			EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1f));
			EditorGUI.DrawRect(drawRect, new Color(0.1f, 0.1f, 0.1f, 1f));

			var timeRange = GetCurveTimeRange(curve);
			var valueRange = GetCurveValueRange(curve);
			var weightRange = new Vector2(UtilityWeightBase.MinValue, UtilityWeightBase.MaxValue);

			DrawGrid(drawRect, timeRange, valueRange);
			DrawRandomCurve(drawRect, curve, timeRange, valueRange);
			DrawWeightCompareThreshold(drawRect, weightCompare, timeRange, valueRange);

			DrawLabels(drawRect, timeRange, valueRange, weightRange);
		}

		private static Vector2 GetCurveTimeRange(AnimationCurve curve)
		{
			if(curve.length == 0)
			{
				return Vector2.zero;
			}
			return new Vector2(curve.keys[0].time, curve.keys[curve.length - 1].time);
		}

		private static Vector2 GetCurveValueRange(AnimationCurve curve)
		{
			if(curve.length == 0)
			{
				return Vector2.zero;
			}

			var min = float.MaxValue;
			var max = float.MinValue;

			for(var i = 0; i < curve.length; i++)
			{
				var value = curve.keys[i].value;
				min = Mathf.Min(min, value);
				max = Mathf.Max(max, value);
			}

			var padding = (max - min) * 0.1f;
			return new Vector2(min - padding, max + padding);
		}

		private static void DrawGrid(Rect rect, Vector2 timeRange, Vector2 valueRange)
		{
			var gridColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

			for(var i = 0; i <= 10; i++)
			{
				var t = i / 10f;
				var x = Mathf.Lerp(rect.x, rect.x + rect.width, t);
				var y = Mathf.Lerp(rect.y, rect.y + rect.height, t);

				EditorGUI.DrawRect(new Rect(x, rect.y, 1, rect.height), gridColor);
				EditorGUI.DrawRect(new Rect(rect.x, y, rect.width, 1), gridColor);
			}
		}

		private static void DrawRandomCurve(Rect rect, AnimationCurve curve, Vector2 timeRange, Vector2 valueRange)
		{
			var curveColor = Color.cyan;
			var resolution = (int)rect.width;

			for(var i = 0; i < resolution - 1; i++)
			{
				var t1 = i / (float)(resolution - 1);
				var t2 = (i + 1) / (float)(resolution - 1);

				var time1 = Mathf.Lerp(timeRange.x, timeRange.y, t1);
				var time2 = Mathf.Lerp(timeRange.x, timeRange.y, t2);

				var value1 = curve.Evaluate(time1);
				var value2 = curve.Evaluate(time2);

				var x1 = rect.x + t1 * rect.width;
				var x2 = rect.x + t2 * rect.width;
				var y1 = rect.y + rect.height - (value1 - valueRange.x) / (valueRange.y - valueRange.x) * rect.height;
				var y2 = rect.y + rect.height - (value2 - valueRange.x) / (valueRange.y - valueRange.x) * rect.height;

				DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), curveColor, 2f);
			}
		}

		private static void DrawWeightCompareThreshold(Rect rect, UtilityWeightCompare weightCompare, Vector2 timeRange, Vector2 valueRange)
		{
			var thresholdValue = weightCompare.ThresholdValue;
			var thresholdColor = Color.yellow;

			var normalizedY = (thresholdValue - valueRange.x) / (valueRange.y - valueRange.x);
			var y = rect.y + rect.height - normalizedY * rect.height;

			DrawLine(new Vector2(rect.x, y), new Vector2(rect.x + rect.width, y), thresholdColor, 2f);

			var labelRect = new Rect(rect.x + rect.width - 60, y - 10, 60, 20);
			EditorGUI.LabelField(labelRect, $"Threshold: {thresholdValue:F2}", new GUIStyle(EditorStyles.label) { normal = { textColor = thresholdColor } });
		}

		private static void DrawLabels(Rect rect, Vector2 timeRange, Vector2 valueRange, Vector2 weightRange)
		{
			var labelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.white } };

			EditorGUI.LabelField(new Rect(rect.x, rect.y + rect.height + 2, 60, 15), $"Time: {timeRange.x:F1}", labelStyle);
			EditorGUI.LabelField(new Rect(rect.x + rect.width - 60, rect.y + rect.height + 2, 60, 15), $"{timeRange.y:F1}", labelStyle);

			EditorGUI.LabelField(new Rect(rect.x - 40, rect.y, 35, 15), $"{valueRange.y:F1}", labelStyle);
			EditorGUI.LabelField(new Rect(rect.x - 40, rect.y + rect.height - 15, 35, 15), $"{valueRange.x:F1}", labelStyle);
		}

		private static void DrawLine(Vector2 start, Vector2 end, Color color, float width)
		{
			var originalColor = GUI.color;
			GUI.color = color;

			var length = Vector2.Distance(start, end);
			var angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

			var center = (start + end) * 0.5f;
			var rect = new Rect(center.x - length * 0.5f, center.y - width * 0.5f, length, width);

			var matrix = GUI.matrix;
			GUIUtility.RotateAroundPivot(angle, center);
			EditorGUI.DrawRect(rect, color);
			GUI.matrix = matrix;

			GUI.color = originalColor;
		}
	}
}
