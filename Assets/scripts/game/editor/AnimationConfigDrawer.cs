using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using game.gameplay_core.characters.config.animation;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using dream_lib.src.utils.editor;
using dream_lib.src.extensions;

namespace game.editor
{
	[CustomPropertyDrawer(typeof(AnimationConfig))]
	public class AnimationConfigDrawer : PropertyDrawer
	{
		private const float LayerHeaderWidth = 120;
		private const float LayerHeight = 35;
		private const float TimelineHeaderHeight = 25;
		private const float ResizeHandleWidth = 6;

		private const float DefaultFrameWidth = 10f;
		private const float MinFrameWidth = 1f;
		private const float MaxFrameWidth = 100f;

		private static readonly Dictionary<string, Vector2> _scrollPositions = new();
		private static readonly Dictionary<string, bool> _showSecondsMap = new();
		private static readonly Dictionary<string, int> _selectedEventMap = new(); // Packed ID: (type << 16) | index. -1 is none.
		private static readonly Dictionary<string, float> _frameWidths = new();
		private static readonly Dictionary<string, PropertyTree> _propertyTreeMap = new();
		private static readonly Dictionary<string, object> _targetObjectMap = new();
		private static readonly Dictionary<string, PreviewAnimationDrawer> _previewDrawers = new();

		private bool _isDragging;
		private bool _isTimelineHandleDragging;
		private int _draggedEventPackedId = -1;
		private int _dragType; // 0: Move, 1: Resize Start, 2: Resize End
		private float _dragOffsetNormalized;
		private float _dragInitialStartNormalized;
		private float _dragInitialEndNormalized;
		private Vector2 _dragInitialMousePos;
		private bool _dragIsVerticalLock;

		private AnimationConfig _config;

		// 0 = Flag, 1 = Hit
		private const int TypeFlag = 0;
		private const int TypeHit = 1;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var clipProp = property.FindPropertyRelative("Clip");
			var speedProp = property.FindPropertyRelative("Speed");
			var flagEventsProp = property.FindPropertyRelative("FlagEvents");
			var hitEventsProp = property.FindPropertyRelative("HitEvents");
			var layerNamesProp = property.FindPropertyRelative("LayerNames");
			
			_config = GetValue(property) as AnimationConfig;
			
			if (_config == null)
			{
				EditorGUILayout.HelpBox("Could not retrieve AnimationConfig instance.", MessageType.Warning);
				EditorGUI.EndProperty();
				return;
			}

			if (!DrawHeader(clipProp, speedProp, out var clip, out var speed))
			{
				EditorGUI.EndProperty();
				return;
			}

			var duration = clip.length / speed;
			var maxFrame = Mathf.RoundToInt(duration * AnimationConfig.EditorPrecisionFps);
			var path = property.propertyPath;

			DrawLayerManagement(layerNamesProp, path, flagEventsProp, hitEventsProp);

			EditorGUILayout.Space();

			var target = property.serializedObject.targetObject;
			var targetKey = AssetDatabase.GetAssetPath(target);
			if (string.IsNullOrEmpty(targetKey)) targetKey = target.GetInstanceID().ToString();
			var uniquePath = targetKey + "_" + path;
			
			var previewDrawer = GetPreviewDrawer(uniquePath, clip);

			// Timeline UI
			DrawTimeline(path, maxFrame, flagEventsProp, hitEventsProp, layerNamesProp, clip.name, previewDrawer);

			// Preview
			EditorGUILayout.Space();
			previewDrawer.WeaponPrefabKey = _config.WeaponForPreview;
			previewDrawer.Draw();

			// Draw Selected Event Inspector
			DrawSelectedEventInspector(flagEventsProp, hitEventsProp, path);

			EditorGUI.EndProperty();
		}

		private PreviewAnimationDrawer GetPreviewDrawer(string path, AnimationClip clip)
		{
			if(!_previewDrawers.TryGetValue(path, out var drawer) || drawer == null)
			{
				drawer = new PreviewAnimationDrawer(AddressableAssetNames.Player, clip);
				_previewDrawers[path] = drawer;
			}

			if(drawer.Clip != clip)
			{
				drawer.Clip = clip;
			}

			return drawer;
		}

		private bool DrawHeader(SerializedProperty clipProp, SerializedProperty speedProp, out AnimationClip clip, out float speed)
		{
			EditorGUILayout.PropertyField(clipProp);
			EditorGUILayout.PropertyField(speedProp);

			clip = clipProp.objectReferenceValue as AnimationClip;
			speed = speedProp.floatValue;

			if(clip == null)
			{
				EditorGUILayout.HelpBox("Assign an Animation Clip to see the timeline.", MessageType.Info);
				return false;
			}

			if(speed <= 0)
			{
				speed = 1f;
			}

			return true;
		}

		private void DrawLayerManagement(SerializedProperty layerNamesProp, string path, SerializedProperty flagEventsProp, SerializedProperty hitEventsProp)
		{
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Timeline Layers", EditorStyles.boldLabel);

			if(!_showSecondsMap.ContainsKey(path))
			{
				_showSecondsMap[path] = false;
			}
			var showSeconds = _showSecondsMap[path];

			if(GUILayout.Button(showSeconds ? "Switch to Frames" : "Switch to Seconds", GUILayout.Width(130)))
			{
				_showSecondsMap[path] = !showSeconds;
			}

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("+ Add Layer", GUILayout.Width(100)))
			{
				layerNamesProp.InsertArrayElementAtIndex(layerNamesProp.arraySize);
				layerNamesProp.GetArrayElementAtIndex(layerNamesProp.arraySize - 1).stringValue = "New Layer";
			}
			EditorGUILayout.EndHorizontal();

			for(var i = 0; i < layerNamesProp.arraySize; i++)
			{
				EditorGUILayout.BeginHorizontal();
				var layerNameProp = layerNamesProp.GetArrayElementAtIndex(i);
				layerNameProp.stringValue = EditorGUILayout.TextField($"Layer {i}", layerNameProp.stringValue);
				if(GUILayout.Button("Delete", GUILayout.Width(60)))
				{
					layerNamesProp.DeleteArrayElementAtIndex(i);
				}
				EditorGUILayout.EndHorizontal();
			}
		}

		private void DrawSelectedEventInspector(SerializedProperty flagEventsProp, SerializedProperty hitEventsProp, string path)
		{
			var packedId = _selectedEventMap.ContainsKey(path) ? _selectedEventMap[path] : -1;
			if (packedId == -1) return;

			var typeId = packedId >> 16;
			var index = packedId & 0xFFFF;

			SerializedProperty activeListChangeCheck = null;
			SerializedProperty eventProp = null;

			if (typeId == TypeFlag && index < flagEventsProp.arraySize)
			{
				eventProp = flagEventsProp.GetArrayElementAtIndex(index);
				activeListChangeCheck = flagEventsProp;
			}
			else if (typeId == TypeHit && index < hitEventsProp.arraySize)
			{
				eventProp = hitEventsProp.GetArrayElementAtIndex(index);
				activeListChangeCheck = hitEventsProp;
			}

			if (eventProp != null)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Selected Event Settings", EditorStyles.boldLabel);

				// Get the actual object instance for this event
				var targetElement = GetValue(eventProp) as AnimationEventBase;
				if(targetElement != null)
				{
					targetElement.ClipDuration = _config.Duration;
					
					if(!_propertyTreeMap.TryGetValue(path, out var tree) || !_targetObjectMap.TryGetValue(path, out var oldTarget) || oldTarget != targetElement)
					{
						tree?.Dispose();
						_propertyTreeMap[path] = tree = PropertyTree.Create(targetElement);
						_targetObjectMap[path] = targetElement;
					}

					tree.UpdateTree();

					EditorGUI.BeginChangeCheck();
					tree.Draw(false);
					if(EditorGUI.EndChangeCheck())
					{
						tree.ApplyChanges();
						EditorUtility.SetDirty(activeListChangeCheck.serializedObject.targetObject);
					}
				}
			}
		}

		private object GetValue(SerializedProperty prop)
		{
			object obj = prop.serializedObject.targetObject;
			var path = prop.propertyPath.Replace(".Array.data[", "[");
			var components = path.Split('.');

			foreach(var component in components)
			{
				var prevObj = obj;
				if(component.Contains("["))
				{
					var name = component.Substring(0, component.IndexOf('['));
					var index = int.Parse(component.Substring(component.IndexOf('[') + 1).Replace("]", ""));
					obj = GetFieldValue(obj, name);
					if(obj is IList list)
					{
						obj = list[index];
					}
				}
				else
				{
					obj = GetFieldValue(obj, component);
				}
			}
			return obj;
		}

		private object GetFieldValue(object obj, string name)
		{
			if(obj == null)
			{
				return null;
			}
			var type = obj.GetType();
			while(type != null)
			{
				var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (field == null)
				{
					// For auto-properties, the backing field is named <Name>k__BackingField
					field = type.GetField($"<{name}>k__BackingField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				}

				if(field != null)
				{
					return field.GetValue(obj);
				}
				type = type.BaseType;
			}
			return null;
		}

		private void DrawTimeline(string path, int maxFrame, SerializedProperty flagEventsProp, SerializedProperty hitEventsProp, SerializedProperty layerNamesProp, string clipName, PreviewAnimationDrawer previewDrawer)
		{
			if(!_scrollPositions.ContainsKey(path))
			{
				_scrollPositions[path] = Vector2.zero;
			}
			var scrollPos = _scrollPositions[path];

			var prefsKey = $"AnimationConfigDrawer_Scale_{clipName}";
			if(!_frameWidths.ContainsKey(clipName))
			{
				_frameWidths[clipName] = EditorPrefs.GetFloat(prefsKey, DefaultFrameWidth);
			}
			var frameWidth = _frameWidths[clipName];

			var totalTimelineWidth = maxFrame * frameWidth;
			var timelineViewHeight = layerNamesProp.arraySize * LayerHeight + TimelineHeaderHeight;

			var controlRect = EditorGUILayout.GetControlRect(false, timelineViewHeight + 20); // Extra space for scrollbar

			// Background
			GUI.Box(controlRect, "", EditorStyles.helpBox);

			// Handle Zoom (Shift + Scroll)
			var e = Event.current;
			if(e.type == EventType.ScrollWheel && e.shift && controlRect.Contains(e.mousePosition))
			{
				var zoomDelta = -e.delta.y * 0.5f;
				var newWidth = Mathf.Clamp(frameWidth + zoomDelta, MinFrameWidth, MaxFrameWidth);
				_frameWidths[clipName] = newWidth;
				EditorPrefs.SetFloat(prefsKey, newWidth);
				e.Use();
				GUI.changed = true;
			}

			// Layer Headers sidebar
			var headersRect = new Rect(controlRect.x, controlRect.y + TimelineHeaderHeight, LayerHeaderWidth, timelineViewHeight - TimelineHeaderHeight);
			GUI.BeginGroup(headersRect);
			for(var i = 0; i < layerNamesProp.arraySize; i++)
			{
				var r = new Rect(0, i * LayerHeight, LayerHeaderWidth, LayerHeight);
				GUI.Box(r, layerNamesProp.GetArrayElementAtIndex(i).stringValue, EditorStyles.label);

				// Add button for event in this layer
				if(GUI.Button(new Rect(LayerHeaderWidth - 25, i * LayerHeight + 5, 20, 20), "+"))
				{
					var layer = i;
					var layerName = layerNamesProp.GetArrayElementAtIndex(layer).stringValue;
					
					if (layerName == AnimationEventLayer.Hits.ToString())
					{
						AddEvent(hitEventsProp, layer, 0, 0.1f, typeof(AnimationEventHit), AnimationEventLayer.Hits);
					}
					else if (layerName == AnimationEventLayer.Combo.ToString())
					{
						var menu = new GenericMenu();
						menu.AddItem(new GUIContent("Exit To Next Combo"), false, () => {
							var evt = AddEvent(flagEventsProp, layer, 0, 0.1f, typeof(AnimationFlagEvent), AnimationEventLayer.Combo);
							evt.FindPropertyRelative("Flag").enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.TimingExitToNextCombo;
							evt.serializedObject.ApplyModifiedProperties();
						});
						menu.AddItem(new GUIContent("Enter From Combo"), false, () => {
							var evt = AddEvent(flagEventsProp, layer, 0, 0.1f, typeof(AnimationFlagEvent), AnimationEventLayer.Combo);
							evt.FindPropertyRelative("Flag").enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.TimingEnterFromCombo;
							evt.serializedObject.ApplyModifiedProperties();
						});
						menu.AddItem(new GUIContent("Enter From Roll"), false, () => {
							var evt = AddEvent(flagEventsProp, layer, 0, 0.1f, typeof(AnimationFlagEvent), AnimationEventLayer.Combo);
							evt.FindPropertyRelative("Flag").enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.TimingEnterFromRoll;
							evt.serializedObject.ApplyModifiedProperties();
						});
						menu.ShowAsContext();
					}
					else
					{
						if (Enum.TryParse<AnimationEventLayer>(layerName, out var layerType))
						{
							AddEvent(flagEventsProp, layer, 0, 0.1f, typeof(AnimationFlagEvent), layerType);
						}
						else
						{
							var menu = new GenericMenu();
							var l = layer;
							menu.AddItem(new GUIContent("Flag"), false, () => AddEvent(flagEventsProp, l, 0, 0.1f, typeof(AnimationFlagEvent), AnimationEventLayer.Hits)); // Hits is dummy here
							menu.AddItem(new GUIContent("Hit"), false, () => AddEvent(hitEventsProp, l, 0, 0.1f, typeof(AnimationEventHit), AnimationEventLayer.Hits));
							menu.ShowAsContext();
						}
					}
				}
			}
			GUI.EndGroup();

			// Scrollable Timeline Area
			var scrollRect = new Rect(controlRect.x + LayerHeaderWidth, controlRect.y, controlRect.width - LayerHeaderWidth, timelineViewHeight + 20);
			var viewRect = new Rect(0, 0, totalTimelineWidth + 50, timelineViewHeight);

			_scrollPositions[path] = GUI.BeginScrollView(scrollRect, scrollPos, viewRect);

			// Grid and Frames
			DrawGrid(path, maxFrame, timelineViewHeight, frameWidth);

			// Events
			HandleEventsInteraction(path, flagEventsProp, hitEventsProp, layerNamesProp.arraySize, frameWidth, maxFrame);
			
			// Timeline Playhead Handle
			DrawTimelineHandle(previewDrawer, totalTimelineWidth, timelineViewHeight, frameWidth, maxFrame);

			GUI.EndScrollView();

			if(GUI.changed)
			{
				flagEventsProp.serializedObject.ApplyModifiedProperties();
				hitEventsProp.serializedObject.ApplyModifiedProperties();
			}
		}

		private void DrawTimelineHandle(PreviewAnimationDrawer previewDrawer, float totalWidth, float height, float frameWidth, int maxFrame)
		{
			var e = Event.current;
			var x = previewDrawer.Time * totalWidth;
			
			// Ruler handle rect
			var handleRect = new Rect(x - 5, 0, 10, TimelineHeaderHeight);
			var rulerRect = new Rect(0, 0, totalWidth, TimelineHeaderHeight);
			EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.SplitResizeLeftRight);

			// Handle interaction
			if (e.type == EventType.MouseDown && rulerRect.Contains(e.mousePosition) && e.button == 0)
			{
				previewDrawer.Time = Mathf.Clamp01(e.mousePosition.x / totalWidth);
				_isTimelineHandleDragging = true;
				GUI.changed = true;
				e.Use();
			}

			if (_isTimelineHandleDragging)
			{
				if (e.type == EventType.MouseDrag)
				{
					previewDrawer.Time = Mathf.Clamp01(e.mousePosition.x / totalWidth);
					GUI.changed = true;
					e.Use();
				}
				else if (e.type == EventType.MouseUp)
				{
					_isTimelineHandleDragging = false;
					e.Use();
				}
			}

			// Draw playhead line
			Handles.BeginGUI();
			var playheadColor = new Color(0.35f, 0.75f, 0.55f, 1.0f);
			var handleColor = new Color(0.35f, 0.75f, 0.55f, 0.9f);
			var outlineColor = new Color(0.1f, 0.25f, 0.15f, 1.0f);

			Handles.color = playheadColor;
			Handles.DrawLine(new Vector3(x, TimelineHeaderHeight, 0), new Vector3(x, height, 0));

			// Draw arrow-shaped handle cap
			var handleHeight = TimelineHeaderHeight;
			var handleWidth = 12f;
			Vector3[] handlePoints = new Vector3[]
			{
				new Vector3(x - handleWidth/2, 0, 0),
				new Vector3(x + handleWidth/2, 0, 0),
				new Vector3(x + handleWidth/2, handleHeight * 0.7f, 0),
				new Vector3(x, handleHeight, 0),
				new Vector3(x - handleWidth/2, handleHeight * 0.7f, 0)
			};
			
			// Fill
			Handles.color = handleColor;
			Handles.DrawAAConvexPolygon(handlePoints);
			
			// Outline
			Handles.color = outlineColor;
			for (int i = 0; i < handlePoints.Length; i++)
			{
				Handles.DrawLine(handlePoints[i], handlePoints[(i + 1) % handlePoints.Length]);
			}
			
			Handles.EndGUI();
		}

		private void DrawGrid(string path, int maxFrame, float height, float frameWidth)
		{
			var showSeconds = _showSecondsMap.ContainsKey(path) && _showSecondsMap[path];
			Handles.BeginGUI();

			var step = showSeconds ? (int)AnimationConfig.EditorTimelineFps : 10;

			// Adaptive step based on zoom
			if(!showSeconds && frameWidth < 3)
			{
				step = 50;
			}
			else if(!showSeconds && frameWidth < 6)
			{
				step = 20;
			}

			if(showSeconds && AnimationConfig.EditorPrecisionFps > 60)
			{
				step = (int)AnimationConfig.EditorTimelineFps; // Show 0.5s if high fps
			}

			for(var f = 0; f <= maxFrame; f += step)
			{
				DrawGridLabel(f, maxFrame, height, frameWidth, path, showSeconds);
			}

			// Force draw last frame if not already drawn
			if (maxFrame % step != 0)
			{
				DrawGridLabel(maxFrame, maxFrame, height, frameWidth, path, showSeconds);
			}

			Handles.EndGUI();
		}

		private void DrawGridLabel(int f, int maxFrame, float height, float frameWidth, string path, bool showSeconds)
		{
			var x = f * frameWidth;
			string label;
			if(showSeconds)
			{
				label = (f / AnimationConfig.EditorPrecisionFps).ToString("F2") + "s";
			}
			else
			{
				// Show frame labels scaled to 60 FPS
				// f is at EditorTimelineFps (e.g. 200)
				var frameAt60 = f / AnimationConfig.EditorPrecisionFps * 60f;
				label = Mathf.RoundToInt(frameAt60).ToString();
			}

			var labelWidth = 50f;
			var labelRect = new Rect(x, 0, labelWidth, TimelineHeaderHeight);
			if (f == maxFrame && f > 0)
			{
				labelRect.x -= labelWidth;
				var style = new GUIStyle(EditorStyles.miniLabel);
				style.alignment = TextAnchor.UpperRight;
				GUI.Label(labelRect, label, style);
			}
			else
			{
				GUI.Label(labelRect, label, EditorStyles.miniLabel);
			}

			Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.2f);
			Handles.DrawLine(new Vector3(x, TimelineHeaderHeight, 0), new Vector3(x, height, 0));

			if(f % (int)AnimationConfig.EditorPrecisionFps == 0 && f > 0) // Indicate seconds
			{
				Handles.color = new Color(1f, 1f, 0, 0.3f);
				Handles.DrawLine(new Vector3(x, 0, 0), new Vector3(x, height, 0));
			}
		}

		private void HandleEventsInteraction(string path, SerializedProperty flagsProp, SerializedProperty hitsProp, int layerCount, float frameWidth, int totalEditorFrames)
		{
			var e = Event.current;
			var hoveredPackedId = -1;
			var hoveredType = -1; // 0: body, 1: start, 2: end

			if(!_selectedEventMap.ContainsKey(path))
			{
				_selectedEventMap[path] = -1;
			}
			var selectedPackedId = _selectedEventMap[path];

			// Key handling
			if(e.type == EventType.KeyDown && (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace))
			{
				if(selectedPackedId != -1)
				{
					var typeId = selectedPackedId >> 16;
					var index = selectedPackedId & 0xFFFF;
					var targetProp = typeId == TypeFlag ? flagsProp : hitsProp;

					if(index < targetProp.arraySize)
					{
						targetProp.DeleteArrayElementAtIndex(index);
						_selectedEventMap[path] = -1;
						targetProp.serializedObject.ApplyModifiedProperties();
						e.Use();
						return;
					}
				}
			}

			// Helper to draw and check hover
			void DrawAndCheckIter(SerializedProperty listProp, int typeId)
			{
				for(var i = 0; i < listProp.arraySize; i++)
				{
					var eventProp = listProp.GetArrayElementAtIndex(i);
					var layerProp = eventProp.FindPropertyRelative("LayerIndex");
					if (layerProp == null) continue;
					
					var layer = layerProp.intValue;
					if(layer >= layerCount) continue;

					var startProp = eventProp.FindPropertyRelative("StartTimeNormalized");
					var endProp = eventProp.FindPropertyRelative("EndTimeNormalized");
					var nameProp = eventProp.FindPropertyRelative("Name");

					if (startProp == null || endProp == null || nameProp == null) continue;
					
					var targetElement = GetValue(eventProp) as AnimationEventBase;
					
					var startTime = startProp.floatValue;
					var endTime = endProp.floatValue;
					var name = targetElement != null ? targetElement.ToString() : nameProp.stringValue;

					var isSingleFrame = targetElement != null && targetElement.IsSingleFrame;
					if (isSingleFrame)
					{
						var oneFrame = 1.0f / (totalEditorFrames > 0 ? totalEditorFrames : 1);
						if (endTime != startTime + oneFrame)
						{
							endTime = startTime + oneFrame;
							endProp.floatValue = endTime;
						}
					}

					var startPx = startTime * totalEditorFrames * frameWidth;
					var endPx = endTime * totalEditorFrames * frameWidth;

					var rect = new Rect(startPx, TimelineHeaderHeight + layer * LayerHeight + 2, endPx - startPx, LayerHeight - 4);

					var isLeftEarHidden = isSingleFrame || startTime <= 0.0001f;
					var isRightEarHidden = isSingleFrame || endTime >= 0.9999f;
					var earVisualWidth = 4f;
					var earHitWidth = 14f;
					
					var leftEarRect = new Rect(rect.x - 2, rect.y, earVisualWidth, rect.height / 2f);
					var rightEarRect = new Rect(rect.xMax - 2, rect.y, earVisualWidth, rect.height / 2f);
					
					var leftEarHitRect = new Rect(rect.x - earHitWidth / 2f, rect.y, earHitWidth, rect.height / 2f);
					var rightEarHitRect = new Rect(rect.xMax - earHitWidth / 2f, rect.y, earHitWidth, rect.height / 2f);

					var currentPackedId = (typeId << 16) | i;
					var isSelected = selectedPackedId == currentPackedId;
					var isDragged = _draggedEventPackedId == currentPackedId;
					
					// Different colors for different types?
					var baseColor = isSelected || isDragged ? new Color(0.3f, 0.6f, 1f, 0.8f) : 
								   (typeId == TypeHit ? new Color(0.8f, 0.3f, 0.3f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.8f));
					
					var earColor = isSelected || isDragged ? new Color(0.5f, 0.8f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);

					GUI.color = baseColor;
					GUI.Box(rect, "", EditorStyles.helpBox);
					
					var leftEarColor = earColor;
					if (isLeftEarHidden) leftEarColor.a = 0;
					GUI.color = leftEarColor;
					GUI.Box(leftEarRect, "", "minibutton");

					var rightEarColor = earColor;
					if (isRightEarHidden) rightEarColor.a = 0;
					GUI.color = rightEarColor;
					GUI.Box(rightEarRect, "", "minibutton");

					GUI.color = Color.white;

					if (!isSingleFrame && leftEarHitRect.Contains(e.mousePosition))
					{
						hoveredPackedId = currentPackedId;
						hoveredType = 1;
					}
					else if (!isSingleFrame && rightEarHitRect.Contains(e.mousePosition))
					{
						hoveredPackedId = currentPackedId;
						hoveredType = 2;
					}
					else if (rect.Contains(e.mousePosition))
					{
						hoveredPackedId = currentPackedId;
						hoveredType = 0;
					}

					// Context click
					if (e.type == EventType.ContextClick && (rect.Contains(e.mousePosition) || 
						leftEarHitRect.Contains(e.mousePosition) || 
						rightEarHitRect.Contains(e.mousePosition)))
					{
						var menu = new GenericMenu();
						var index = i;
						menu.AddItem(new GUIContent("Delete"), false, () =>
						{
							listProp.DeleteArrayElementAtIndex(index);
							_selectedEventMap[path] = -1;
							listProp.serializedObject.ApplyModifiedProperties();
						});
						menu.ShowAsContext();
						e.Use();
					}
					
					if(isSelected)
					{
						// Highlight
					}

					var labelStyle = new GUIStyle(EditorStyles.miniLabel);
					var labelContent = new GUIContent(name);
					var labelWidth = labelStyle.CalcSize(labelContent).x;
					var labelRect = new Rect(rect.x + 5, rect.y, rect.width - 10, rect.height);
					
					if (hoveredPackedId == currentPackedId && rect.width < labelWidth + 10)
					{
						labelRect.width = labelWidth + 10;
						var bgRect = labelRect;
						bgRect.x -= 2;
						bgRect.width += 4;
						GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
						GUI.Box(bgRect, "", EditorStyles.helpBox);
						GUI.color = Color.white;
					}
					
					GUI.Label(labelRect, labelContent, labelStyle);
				}
			}

			DrawAndCheckIter(flagsProp, TypeFlag);
			DrawAndCheckIter(hitsProp, TypeHit);

			// Cursor for hovered
			if (hoveredPackedId != -1)
			{
				var t = hoveredPackedId >> 16;
				var idx = hoveredPackedId & 0xFFFF;
				var list = t == TypeFlag ? flagsProp : hitsProp;
				if(idx < list.arraySize)
				{
					var p = list.GetArrayElementAtIndex(idx);
					var startTime = p.FindPropertyRelative("StartTimeNormalized").floatValue;
					var endTime = p.FindPropertyRelative("EndTimeNormalized").floatValue;
					var layer = p.FindPropertyRelative("LayerIndex").intValue;
					
					var startPx = startTime * totalEditorFrames * frameWidth;
					var endPx = endTime * totalEditorFrames * frameWidth;
					var r = new Rect(startPx, TimelineHeaderHeight + layer * LayerHeight + 2, endPx - startPx, LayerHeight - 4);
					
					var targetElement = GetValue(p) as AnimationEventBase;
					var isSingleFrame = targetElement != null && targetElement.IsSingleFrame;
					
					if (hoveredType == 1 && !isSingleFrame) 
						EditorGUIUtility.AddCursorRect(new Rect(r.x - 7, r.y, 14, r.height / 2f), MouseCursor.ResizeHorizontal);
					else if (hoveredType == 2 && !isSingleFrame) 
						EditorGUIUtility.AddCursorRect(new Rect(r.xMax - 7, r.y, 14, r.height / 2f), MouseCursor.ResizeHorizontal);
					else 
						EditorGUIUtility.AddCursorRect(r, MouseCursor.MoveArrow);
				}
			}
			
			// Dragging Logic
			switch(e.type)
			{
				case EventType.MouseDown:
					if (hoveredPackedId != -1 && e.button == 0)
					{
						var t = hoveredPackedId >> 16;
						var idx = hoveredPackedId & 0xFFFF;
						var list = t == TypeFlag ? flagsProp : hitsProp;
						var p = list.GetArrayElementAtIndex(idx);
						var targetElement = GetValue(p) as AnimationEventBase;
						if (targetElement != null && targetElement.IsSingleFrame && hoveredType != 0)
						{
							return;
						}

						_isDragging = true;
						_draggedEventPackedId = hoveredPackedId;
						_selectedEventMap[path] = hoveredPackedId;
						_dragType = hoveredType;

						var startProp = p?.FindPropertyRelative("StartTimeNormalized");
						var endProp = p?.FindPropertyRelative("EndTimeNormalized");

						if (startProp != null && endProp != null)
						{
							_dragInitialStartNormalized = startProp.floatValue;
							_dragInitialEndNormalized = endProp.floatValue;
							_dragOffsetNormalized = e.mousePosition.x / (frameWidth * totalEditorFrames);
							_dragInitialMousePos = e.mousePosition;
							_dragIsVerticalLock = false;
							e.Use();
						}
						else
						{
							_isDragging = false;
							_draggedEventPackedId = -1;
						}
					}
					else if(e.button == 0)
					{
						_selectedEventMap[path] = -1;
					}
					break;

				case EventType.MouseDrag:
					if(_isDragging && _draggedEventPackedId != -1)
					{
						var t = _draggedEventPackedId >> 16;
						var idx = _draggedEventPackedId & 0xFFFF;
						var list = t == TypeFlag ? flagsProp : hitsProp;

						if(idx >= list.arraySize) 
						{
							_isDragging = false;
							return;
						}

						var currentNormalized = e.mousePosition.x / (frameWidth * totalEditorFrames);
						var delta = currentNormalized - _dragOffsetNormalized;

						if (_dragType == 0 && !_dragIsVerticalLock)
						{
							var totalDelta = e.mousePosition - _dragInitialMousePos;
							if (Mathf.Abs(totalDelta.y) > 10f && Mathf.Abs(totalDelta.y) > Mathf.Abs(totalDelta.x) * 2f)
							{
								_dragIsVerticalLock = true;
							}
						}

						if (_dragIsVerticalLock) delta = 0;

						var p = list.GetArrayElementAtIndex(idx);
						var startProp = p?.FindPropertyRelative("StartTimeNormalized");
						var endProp = p?.FindPropertyRelative("EndTimeNormalized");
						var layerProp = p?.FindPropertyRelative("LayerIndex");

						if (startProp == null || endProp == null)
						{
							return;
						}

						if(_dragType == 0) // Move
						{
							var newStart = _dragInitialStartNormalized + delta;
							var newEnd = _dragInitialEndNormalized + delta;
							if(newStart < 0)
							{
								newEnd -= newStart;
								newStart = 0;
							}
							if(newEnd > 1)
							{
								newStart -= newEnd - 1;
								newEnd = 1;
							}
							startProp.floatValue = newStart;
							endProp.floatValue = newEnd;

							if (layerProp != null)
							{
								var newLayer = Mathf.FloorToInt((e.mousePosition.y - TimelineHeaderHeight) / LayerHeight);
								layerProp.intValue = Mathf.Clamp(newLayer, 0, layerCount - 1);
							}
						}
						else if(_dragType == 1) // Resize Start
						{
							startProp.floatValue = Mathf.Clamp(_dragInitialStartNormalized + delta, 0, endProp.floatValue - 0.001f);
						}
						else if(_dragType == 2) // Resize End
						{
							endProp.floatValue = Mathf.Clamp(_dragInitialEndNormalized + delta, startProp.floatValue + 0.001f, 1);
						}

						GUI.changed = true;
						e.Use();
					}
					break;

				case EventType.MouseUp:
					_isDragging = false;
					_draggedEventPackedId = -1;
					break;
			}
		}

		private SerializedProperty AddEvent(SerializedProperty eventsProp, int layerIndex, float startTime, float duration, System.Type type, AnimationEventLayer layerCategory)
		{
			eventsProp.InsertArrayElementAtIndex(eventsProp.arraySize);
			var newEvent = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
			
			var nameProp = newEvent.FindPropertyRelative("Name");
			var layerProp = newEvent.FindPropertyRelative("LayerIndex");
			var startProp = newEvent.FindPropertyRelative("StartTimeNormalized");
			var endProp = newEvent.FindPropertyRelative("EndTimeNormalized");

			if (nameProp != null) nameProp.stringValue = "New " + (type?.Name.Replace("Animation", "").Replace("Event", "") ?? "Event");
			if (layerProp != null) layerProp.intValue = layerIndex;
			if (startProp != null) startProp.floatValue = startTime;
			if (endProp != null) endProp.floatValue = startTime + duration;

			if (type == typeof(AnimationFlagEvent))
			{
				var flagProp = newEvent.FindPropertyRelative("Flag");
				if (flagProp != null)
				{
					switch (layerCategory)
					{
						case AnimationEventLayer.RotationLocked: flagProp.enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.RotationLocked; break;
						case AnimationEventLayer.StateLocked: flagProp.enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.StateLocked; break;
						case AnimationEventLayer.StaminaRegenDisabled: flagProp.enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.StaminaRegenDisabled; break;
						case AnimationEventLayer.BodyAttack: flagProp.enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.BodyAttack; break;
						case AnimationEventLayer.TimingExitToAttack: flagProp.enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.TimingExitToAttack; break;
						case AnimationEventLayer.Invulnerability: flagProp.enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.Invulnerability; break;
						case AnimationEventLayer.Combo: flagProp.enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.TimingExitToNextCombo; break;
						case AnimationEventLayer.Markers: flagProp.enumValueIndex = (int)AnimationFlagEvent.AnimationFlags.StartHandleObstacleCast; break;
					}
				}
			}
			
			eventsProp.serializedObject.ApplyModifiedProperties();
			return newEvent;
		}
	}
}
