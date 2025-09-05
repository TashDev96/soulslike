#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using dream_lib.src.reactive;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace game.gameplay_core.characters.config.Editor
{
	[CustomEditor(typeof(CharacterConfig))]
	public class CharacterConfigEditor : OdinEditor
	{
		private CharacterConfig config;

		protected override void OnEnable()
		{
			base.OnEnable();
			config = target as CharacterConfig;
		}

		public override void OnInspectorGUI()
		{
			if (config == null) return;

			DrawDefaultInspector();

			if (config.ParentConfig != null)
			{
				EditorGUILayout.Space();
				DrawFieldDifferencesSection();
			}
		}

		private void DrawFieldDifferencesSection()
		{
			var differences = GetFieldDifferences();
			
			if (differences.Count == 0) return;

			EditorGUILayout.LabelField("Field Differences", EditorStyles.boldLabel);
			
			EditorGUILayout.BeginVertical("box");
			
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply All to Parent"))
			{
				ApplyAllToParent();
			}
			
			if (GUILayout.Button("Revert All from Parent"))
			{
				RevertAllFromParent();
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.Space();
			
			var detailedDifferences = GetDetailedDifferences();
			foreach (var diff in detailedDifferences)
			{
				EditorGUILayout.BeginHorizontal("box");
				
				EditorGUILayout.LabelField(diff.FieldName, GUILayout.Width(150));
				EditorGUILayout.LabelField($"Current: {diff.CurrentValue}", GUILayout.Width(200));
				EditorGUILayout.LabelField($"Parent: {diff.ParentValue}", GUILayout.Width(200));
				
				if (GUILayout.Button("Apply", GUILayout.Width(60)))
				{
					ApplyFieldToParent(diff.FieldName);
				}
				
				if (GUILayout.Button("Revert", GUILayout.Width(60)))
				{
					RevertFieldFromParent(diff.FieldName);
				}
				
				EditorGUILayout.EndHorizontal();
			}
			
			EditorGUILayout.EndVertical();
		}

		private void ApplyAllToParent()
		{
			if (config.ParentConfig == null) return;

			var differences = GetFieldDifferences();
			foreach (var diff in differences)
			{
				ApplyFieldToParent(diff.Key);
			}
		}

		private void RevertAllFromParent()
		{
			if (config.ParentConfig == null) return;

			var differences = GetFieldDifferences();
			foreach (var diff in differences)
			{
				RevertFieldFromParent(diff.Key);
			}
		}

		private Dictionary<string, object> GetFieldDifferences()
		{
			if (config.ParentConfig == null) return new Dictionary<string, object>();

			var differences = new Dictionary<string, object>();
			CollectFieldDifferences(config, config.ParentConfig, "", differences);
			return differences;
		}

		private void CollectFieldDifferences(object thisObj, object parentObj, string basePath, Dictionary<string, object> differences)
		{
			if (thisObj == null || parentObj == null) return;
			if (thisObj.GetType() != parentObj.GetType()) return;

			var type = thisObj.GetType();
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(f => f.GetCustomAttribute<SerializeField>() != null && !IsParentConfigField(f.Name));

			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(p => p.GetCustomAttribute<SerializeField>() != null && !IsParentConfigField(p.Name));

			foreach (var field in fields)
			{
				var fieldPath = string.IsNullOrEmpty(basePath) ? CleanFieldName(field.Name) : $"{basePath}.{CleanFieldName(field.Name)}";
				var thisValue = field.GetValue(thisObj);
				var parentValue = field.GetValue(parentObj);

				if (IsInlineSerializedType(field.FieldType))
				{
					CollectFieldDifferences(thisValue, parentValue, fieldPath, differences);
				}
				else if (!AreValuesEqual(thisValue, parentValue))
				{
					differences[fieldPath] = thisValue;
				}
			}

			foreach (var prop in properties)
			{
				var propPath = string.IsNullOrEmpty(basePath) ? CleanFieldName(prop.Name) : $"{basePath}.{CleanFieldName(prop.Name)}";
				var thisValue = prop.GetValue(thisObj);
				var parentValue = prop.GetValue(parentObj);

				if (IsInlineSerializedType(prop.PropertyType))
				{
					CollectFieldDifferences(thisValue, parentValue, propPath, differences);
				}
				else if (!AreValuesEqual(thisValue, parentValue))
				{
					differences[propPath] = thisValue;
				}
			}
		}

		private string CleanFieldName(string fieldName)
		{
			if (fieldName.StartsWith("<") && fieldName.Contains(">k__BackingField"))
			{
				var start = fieldName.IndexOf('<') + 1;
				var end = fieldName.IndexOf('>');
				return fieldName.Substring(start, end - start);
			}
			return fieldName;
		}

		private bool IsParentConfigField(string fieldName)
		{
			return fieldName == "ParentConfig" || 
				   fieldName == "parentConfig" || 
				   fieldName.Contains("<ParentConfig>k__BackingField");
		}

		private bool IsInlineSerializedType(System.Type type)
		{
			if (type == null) return false;
			
			if (type.IsValueType || type == typeof(string)) return false;
			
			if (typeof(UnityEngine.Object).IsAssignableFrom(type)) return false;
			
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ReactiveProperty<>)) return false;
			
			return type.IsClass && type.IsSerializable;
		}

		private bool AreValuesEqual(object value1, object value2)
		{
			if (value1 == null && value2 == null) return true;
			if (value1 == null || value2 == null) return false;

			if (value1.GetType().IsGenericType && value1.GetType().GetGenericTypeDefinition() == typeof(ReactiveProperty<>))
			{
				var valueProperty1 = value1.GetType().GetProperty("Value");
				var valueProperty2 = value2.GetType().GetProperty("Value");
				
				if (valueProperty1 != null && valueProperty2 != null)
				{
					var val1 = valueProperty1.GetValue(value1);
					var val2 = valueProperty2.GetValue(value2);
					
					return AreValuesEqual(val1, val2);
				}
			}

			if (IsValueType(value1) && IsValueType(value2))
			{
				return object.Equals(value1, value2);
			}

			if (value1 is AnimationCurve curve1 && value2 is AnimationCurve curve2)
			{
				return AreAnimationCurvesEqual(curve1, curve2);
			}

			if (value1 is UnityEngine.Object unityObj1 && value2 is UnityEngine.Object unityObj2)
			{
				return unityObj1 == unityObj2;
			}

			return object.ReferenceEquals(value1, value2);
		}

		private bool AreAnimationCurvesEqual(AnimationCurve curve1, AnimationCurve curve2)
		{
			if (curve1 == null && curve2 == null) return true;
			if (curve1 == null || curve2 == null) return false;

			if (curve1.keys.Length != curve2.keys.Length) return false;

			for (int i = 0; i < curve1.keys.Length; i++)
			{
				var key1 = curve1.keys[i];
				var key2 = curve2.keys[i];

				if (!Mathf.Approximately(key1.time, key2.time) ||
					!Mathf.Approximately(key1.value, key2.value) ||
					!Mathf.Approximately(key1.inTangent, key2.inTangent) ||
					!Mathf.Approximately(key1.outTangent, key2.outTangent) ||
					key1.inWeight != key2.inWeight ||
					key1.outWeight != key2.outWeight ||
					key1.weightedMode != key2.weightedMode)
				{
					return false;
				}
			}

			return curve1.preWrapMode == curve2.preWrapMode && 
				   curve1.postWrapMode == curve2.postWrapMode;
		}

		private bool IsValueType(object value)
		{
			if (value == null) return false;
			
			var type = value.GetType();
			return type.IsValueType || type == typeof(string);
		}

		private List<FieldDifference> GetDetailedDifferences()
		{
			if (config.ParentConfig == null) return new List<FieldDifference>();

			var differences = new List<FieldDifference>();
			var fieldDiffs = GetFieldDifferences();

			foreach (var diff in fieldDiffs)
			{
				var currentValue = GetDisplayValue(diff.Value);
				var parentValue = GetParentDisplayValue(diff.Key);

				differences.Add(new FieldDifference
				{
					FieldName = diff.Key,
					CurrentValue = currentValue,
					ParentValue = parentValue
				});
			}

			return differences;
		}

		private string GetDisplayValue(object value)
		{
			if (value == null) return "null";

			if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(ReactiveProperty<>))
			{
				var valueProperty = value.GetType().GetProperty("Value");
				if (valueProperty != null)
				{
					var innerValue = valueProperty.GetValue(value);
					return GetDisplayValue(innerValue);
				}
			}

			if (value is AnimationCurve curve)
			{
				if (curve.keys.Length == 0) return "Empty Curve";
				var firstKey = curve.keys[0];
				var lastKey = curve.keys[curve.keys.Length - 1];
				return $"Curve ({curve.keys.Length} keys, {firstKey.time:F2}-{lastKey.time:F2})";
			}

			if (value is UnityEngine.Object unityObj)
			{
				return unityObj != null ? unityObj.name : "null";
			}

			return value.ToString();
		}

		private string GetParentDisplayValue(string fieldPath)
		{
			if (config.ParentConfig == null) return "null";

			try
			{
				var parts = fieldPath.Split('.');
				object current = config.ParentConfig;

				foreach (var part in parts)
				{
					if (current == null) return "null";

					var field = FindField(current.GetType(), part);
					if (field != null)
					{
						current = field.GetValue(current);
						continue;
					}

					var property = FindProperty(current.GetType(), part);
					if (property != null)
					{
						current = property.GetValue(current);
						continue;
					}

					return "field not found";
				}

				return GetDisplayValue(current);
			}
			catch
			{
				return "error getting value";
			}
		}

		private void ApplyFieldToParent(string fieldPath)
		{
			if (config.ParentConfig == null) return;

			try
			{
				var parts = fieldPath.Split('.');
				var value = GetNestedValue(config, parts);
				SetNestedValue(config.ParentConfig, parts, value);
				EditorUtility.SetDirty(config.ParentConfig);
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Failed to apply field {fieldPath} to parent: {e.Message}");
			}
		}

		private void RevertFieldFromParent(string fieldPath)
		{
			if (config.ParentConfig == null) return;

			try
			{
				var parts = fieldPath.Split('.');
				var value = GetNestedValue(config.ParentConfig, parts);
				SetNestedValue(config, parts, value);
				EditorUtility.SetDirty(config);
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Failed to revert field {fieldPath} from parent: {e.Message}");
			}
		}

		private object GetNestedValue(object obj, string[] fieldParts)
		{
			object current = obj;
			foreach (var part in fieldParts)
			{
				if (current == null) return null;

				var field = FindField(current.GetType(), part);
				if (field != null)
				{
					current = field.GetValue(current);
					continue;
				}

				var property = FindProperty(current.GetType(), part);
				if (property != null)
				{
					current = property.GetValue(current);
					continue;
				}

				throw new System.Exception($"Field or property '{part}' not found");
			}
			return current;
		}

		private void SetNestedValue(object obj, string[] fieldParts, object value)
		{
			if (fieldParts.Length == 1)
			{
				var field = FindField(obj.GetType(), fieldParts[0]);
				if (field != null)
				{
					field.SetValue(obj, value);
					return;
				}

				var property = FindProperty(obj.GetType(), fieldParts[0]);
				if (property != null && property.CanWrite)
				{
					property.SetValue(obj, value);
					return;
				}

				throw new System.Exception($"Cannot set field or property '{fieldParts[0]}'");
			}

			var parentParts = fieldParts.Take(fieldParts.Length - 1).ToArray();
			var parent = GetNestedValue(obj, parentParts);
			var lastPart = fieldParts.Last();

			if (parent != null && value != null && 
				parent.GetType().IsGenericType && parent.GetType().GetGenericTypeDefinition() == typeof(ReactiveProperty<>) &&
				value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(ReactiveProperty<>))
			{
				var parentValueProp = parent.GetType().GetProperty("Value");
				var sourceValueProp = value.GetType().GetProperty("Value");
				
				if (parentValueProp != null && sourceValueProp != null)
				{
					var sourceValue = sourceValueProp.GetValue(value);
					parentValueProp.SetValue(parent, sourceValue);
					return;
				}
			}

			SetNestedValue(parent, new[] { lastPart }, value);
		}

		private FieldInfo FindField(System.Type type, string fieldName)
		{
			var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			if (field != null) return field;
			
			var backingFieldName = $"<{fieldName}>k__BackingField";
			return type.GetField(backingFieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		}

		private PropertyInfo FindProperty(System.Type type, string propertyName)
		{
			return type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		}

		[System.Serializable]
		public class FieldDifference
		{
			public string FieldName;
			public string CurrentValue;
			public string ParentValue;
		}
	}
} 
#endif