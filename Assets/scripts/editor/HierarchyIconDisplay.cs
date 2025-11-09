using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace editor
{
	[InitializeOnLoad]
	public static class HierarchyIconDisplay
	{
		private const string PrefsKey = "HierarchyIconDisplay.Enabled";

		private static bool _hierarchyHasFocus;
		private static EditorWindow _hierarchyEditorWindow;
		private static readonly Texture2D DefaultScriptIcon;
		private static bool _enabled;

		static HierarchyIconDisplay()
		{
			_enabled = EditorPrefs.GetBool(PrefsKey, true);
			EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
			EditorApplication.update += OnEditorUpdate;
			DefaultScriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;
		}

		[MenuItem("Tools/Toggle Hierarchy Icon Display", false, 1)]
		private static void ToggleHierarchyIconDisplay()
		{
			_enabled = !_enabled;
			EditorPrefs.SetBool(PrefsKey, _enabled);
		}

		[MenuItem("Tools/Toggle Hierarchy Icon Display", true)]
		private static bool ToggleHierarchyIconDisplayValidate()
		{
			Menu.SetChecked("Tools/Toggle Hierarchy Icon Display", _enabled);
			return true;
		}

		private static void OnEditorUpdate()
		{
			if(_hierarchyEditorWindow == null)
			{
				_hierarchyEditorWindow = EditorWindow.GetWindow(Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor"));
			}

			_hierarchyHasFocus = EditorWindow.focusedWindow != null &&
			                     EditorWindow.focusedWindow == _hierarchyEditorWindow;
		}

		private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
		{
			if(!_enabled)
			{
				return;
			}

			var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
			if(obj == null)
			{
				return;
			}

			if(PrefabUtility.IsPartOfPrefabInstance(obj) && PrefabUtility.GetOutermostPrefabInstanceRoot(obj) == obj)
			{
				return;
			}

			var components = obj.GetComponents<Component>();
			if(components == null || components.Length == 0)
			{
				return;
			}

			var component = components.Length > 1 ? components[1] : components[0];

			var type = component.GetType();

			var content = EditorGUIUtility.ObjectContent(null, type);
			content.text = null;
			content.tooltip = type.Name;

			if(content.image == null)
			{
				return;
			}

			if(content.image == DefaultScriptIcon)
			{
				return;
			}

			var isSelected = Selection.instanceIDs.Contains(instanceID);
			var isHovering = selectionRect.Contains(Event.current.mousePosition);

			var color = UnityEditorBackgroundColor.Get(isSelected, isHovering, _hierarchyHasFocus);
			var backgroundRect = selectionRect;
			backgroundRect.width = 18.5f;
			EditorGUI.DrawRect(backgroundRect, color);

			EditorGUI.LabelField(selectionRect, content);
		}
	}

	public static class UnityEditorBackgroundColor
	{
		private static readonly Color32 SelectedFocused = new(44, 93, 135, 255);
		private static readonly Color32 SelectedUnfocused = new(72, 72, 72, 255);
		private static readonly Color32 Hovering = new(70, 70, 70, 255);
		private static readonly Color32 Normal = new(56, 56, 56, 255);

		public static Color Get(bool selected, bool hovering, bool hierarchyHasFocus)
		{
			if(selected)
			{
				return hierarchyHasFocus ? SelectedFocused : SelectedUnfocused;
			}

			if(hovering)
			{
				return Hovering;
			}

			return Normal;
		}
	}
}