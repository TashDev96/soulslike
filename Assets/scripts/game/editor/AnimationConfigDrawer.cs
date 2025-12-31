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
		private static readonly Dictionary<string, int> _selectedEventMap = new();
		private static readonly Dictionary<string, float> _frameWidths = new();
		private static readonly Dictionary<string, PropertyTree> _propertyTreeMap = new();
		private static readonly Dictionary<string, object> _targetObjectMap = new();
		private static readonly Dictionary<string, PreviewAnimationDrawer> _previewDrawers = new();

		private bool _isDragging;
		private bool _isTimelineHandleDragging;
		private static List<Type> _cachedEventTypes;
		private int _draggedEventIndex = -1;
		private int _dragType; // 0: Move, 1: Resize Start, 2: Resize End
		private float _dragOffsetNormalized;
		private float _dragInitialStartNormalized;
		private float _dragInitialEndNormalized;

		private SerializedProperty _activeEventsProp;
		private AnimationConfig _config;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var clipProp = property.FindPropertyRelative("Clip");
			var speedProp = property.FindPropertyRelative("Speed");
			var eventsProp = property.FindPropertyRelative("Events");
			var layerNamesProp = property.FindPropertyRelative("LayerNames");
			
			_config = GetValue(property) as AnimationConfig;

			if (!DrawHeader(clipProp, speedProp, out var clip, out var speed))
			{
				EditorGUI.EndProperty();
				return;
			}

			var duration = clip.length / speed;
			var maxFrame = Mathf.RoundToInt(duration * AnimationConfig.EditorPrecisionFps);
			var path = property.propertyPath;

			DrawLayerManagement(layerNamesProp, path);

			EditorGUILayout.Space();

			var previewDrawer = GetPreviewDrawer(path, clip);

			// Timeline UI
			DrawTimeline(path, maxFrame, eventsProp, layerNamesProp, clip.name, previewDrawer);

			// Preview
			EditorGUILayout.Space();
			previewDrawer.Draw();

			// Draw Selected Event Inspector
			DrawSelectedEventInspector(eventsProp, path);

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

		private void DrawLayerManagement(SerializedProperty layerNamesProp, string path)
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

		private void DrawSelectedEventInspector(SerializedProperty eventsProp, string path)
		{
			var selectedIndex = _selectedEventMap.ContainsKey(path) ? _selectedEventMap[path] : -1;
			if(selectedIndex >= 0 && selectedIndex < eventsProp.arraySize)
			{
				EditorGUILayout.Space();
				EditorGUILayout.LabelField("Selected Event Settings", EditorStyles.boldLabel);

				var eventProp = eventsProp.GetArrayElementAtIndex(selectedIndex);
				if (eventProp == null) return;

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
						EditorUtility.SetDirty(eventsProp.serializedObject.targetObject);
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
				if(field != null)
				{
					return field.GetValue(obj);
				}
				type = type.BaseType;
			}
			return null;
		}

		private void DrawTimeline(string path, int maxFrame, SerializedProperty eventsProp, SerializedProperty layerNamesProp, string clipName, PreviewAnimationDrawer previewDrawer)
		{
			_activeEventsProp = eventsProp;

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
					var menu = new GenericMenu();
					var layer = i;
					
					if (_cachedEventTypes == null)
					{
						_cachedEventTypes = new List<Type> { typeof(AnimationEventBase) };
						var derivedTypes = TypeCache.GetTypesDerivedFrom<AnimationEventBase>();
						foreach (var type in derivedTypes)
						{
							if (!type.IsAbstract)
								_cachedEventTypes.Add(type);
						}
					}

					foreach (var type in _cachedEventTypes)
					{
						var typeName = type.Name.Replace("AnimationEvent", "").Replace("Event", "");
						if (string.IsNullOrEmpty(typeName)) typeName = "Base";
						menu.AddItem(new GUIContent(typeName), false, () => AddEvent(eventsProp, layer, 0, 0.1f, type));
					}
					
					menu.ShowAsContext();
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
			HandleEventsInteraction(path, eventsProp, layerNamesProp.arraySize, frameWidth);
			
			// Timeline Playhead Handle
			DrawTimelineHandle(previewDrawer, totalTimelineWidth, timelineViewHeight, frameWidth, maxFrame);

			GUI.EndScrollView();

			if(GUI.changed)
			{
				eventsProp.serializedObject.ApplyModifiedProperties();
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

		private void HandleEventsInteraction(string path, SerializedProperty eventsProp, int layerCount, float frameWidth)
		{
			// Get duration for normalized <-> frame conversion
			var clipProp = eventsProp.serializedObject.FindProperty(_activeEventsProp.propertyPath.Replace(".Events", ".Clip"));
			var speedProp = eventsProp.serializedObject.FindProperty(_activeEventsProp.propertyPath.Replace(".Events", ".Speed"));
			var clip = clipProp?.objectReferenceValue as AnimationClip;
			var speed = speedProp != null ? speedProp.floatValue : 1f;
			if(speed <= 0)
			{
				speed = 1f;
			}
			var duration = clip != null ? clip.length / speed : 1f;
			var totalEditorFrames = duration * AnimationConfig.EditorPrecisionFps;

			var e = Event.current;
			var hoveredEvent = -1;
			var hoveredType = -1; // 0: body, 1: start, 2: end

			if(!_selectedEventMap.ContainsKey(path))
			{
				_selectedEventMap[path] = -1;
			}
			var selectedIndex = _selectedEventMap[path];

			// Key handling
			if(e.type == EventType.KeyDown && (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace))
			{
				if(selectedIndex != -1 && selectedIndex < eventsProp.arraySize)
				{
					eventsProp.DeleteArrayElementAtIndex(selectedIndex);
					_selectedEventMap[path] = -1;
					eventsProp.serializedObject.ApplyModifiedProperties();
					e.Use();
					return;
				}
			}

			// Draw events and find hover
			for(var i = 0; i < eventsProp.arraySize; i++)
			{
				var eventProp = eventsProp.GetArrayElementAtIndex(i);
				var layerProp = eventProp.FindPropertyRelative("LayerIndex");
				if (layerProp == null) continue;
				
				var layer = layerProp.intValue;
				if(layer >= layerCount)
				{
					continue;
				}

				var startProp = eventProp.FindPropertyRelative("StartTimeNormalized");
				var endProp = eventProp.FindPropertyRelative("EndTimeNormalized");
				var nameProp = eventProp.FindPropertyRelative("Name");

				if (startProp == null || endProp == null || nameProp == null) continue;
				
				var targetElement = GetValue(eventProp) as AnimationEventBase;
				

				var startTime = startProp.floatValue;
				var endTime = endProp.floatValue;
				var name = targetElement.ToString();

				var startPx = startTime * totalEditorFrames * frameWidth;
				var endPx = endTime * totalEditorFrames * frameWidth;

				var rect = new Rect(startPx, TimelineHeaderHeight + layer * LayerHeight + 2, endPx - startPx, LayerHeight - 4);

				// Ears hit detection and visual
				var showLeftEar = startTime > 0.0001f;
				var showRightEar = endTime < 0.9999f;
				var earVisualWidth = 4f;
				var earHitWidth = 14f;
				// Position 2px closer (centered on edges)
				var leftEarRect = new Rect(rect.x - 2, rect.y, earVisualWidth, rect.height);
				var rightEarRect = new Rect(rect.xMax - 2, rect.y, earVisualWidth, rect.height);
				
				var leftEarHitRect = new Rect(rect.x - earHitWidth / 2f, rect.y, earHitWidth, rect.height);
				var rightEarHitRect = new Rect(rect.xMax - earHitWidth / 2f, rect.y, earHitWidth, rect.height);

				var isSelected = selectedIndex == i;
				var isDragged = _draggedEventIndex == i;
				var baseColor = isSelected || isDragged ? new Color(0.3f, 0.6f, 1f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);
				var earColor = isSelected || isDragged ? new Color(0.5f, 0.8f, 1f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);

				// Draw Event Body
				GUI.color = baseColor;
				GUI.Box(rect, "", EditorStyles.helpBox);
				
				// Draw Ears (Rounded)
				GUI.color = earColor;
				if (showLeftEar) GUI.Box(leftEarRect, "", "minibutton");
				if (showRightEar) GUI.Box(rightEarRect, "", "minibutton");
				GUI.color = Color.white;

				// Handle mouse hover/interactions (Prioritize ears of later drawn events)
				if (showLeftEar && leftEarHitRect.Contains(e.mousePosition))
				{
					hoveredEvent = i;
					hoveredType = 1;
				}
				else if (showRightEar && rightEarHitRect.Contains(e.mousePosition))
				{
					hoveredEvent = i;
					hoveredType = 2;
				}
				else if (rect.Contains(e.mousePosition))
				{
					hoveredEvent = i;
					hoveredType = 0;
				}

				// Right click to delete
				if (e.type == EventType.ContextClick && (rect.Contains(e.mousePosition) || (showLeftEar && leftEarHitRect.Contains(e.mousePosition)) || (showRightEar && rightEarHitRect.Contains(e.mousePosition))))
				{
					var menu = new GenericMenu();
					var index = i;
					menu.AddItem(new GUIContent("Delete"), false, () =>
					{
						_activeEventsProp.DeleteArrayElementAtIndex(index);
						_selectedEventMap[path] = -1;
						_activeEventsProp.serializedObject.ApplyModifiedProperties();
					});
					menu.ShowAsContext();
					e.Use();
				}

				if(isSelected)
				{
					// Highlight is enough, we draw the inspector below
				}

				GUI.Label(new Rect(rect.x + 5, rect.y, rect.width - 10, rect.height), name, EditorStyles.miniLabel);
			}

			// Add cursor rect for the current hovered item (to ensure it wins over other UI)
			if (hoveredEvent != -1)
			{
				var p = eventsProp.GetArrayElementAtIndex(hoveredEvent);
				var startTime = p.FindPropertyRelative("StartTimeNormalized").floatValue;
				var endTime = p.FindPropertyRelative("EndTimeNormalized").floatValue;
				var layer = p.FindPropertyRelative("LayerIndex").intValue;
				
				var startPx = startTime * totalEditorFrames * frameWidth;
				var endPx = endTime * totalEditorFrames * frameWidth;
				var r = new Rect(startPx, TimelineHeaderHeight + layer * LayerHeight + 2, endPx - startPx, LayerHeight - 4);
				
				if (hoveredType == 1) // Start
					EditorGUIUtility.AddCursorRect(new Rect(r.x - 7, r.y, 14, r.height), MouseCursor.ResizeHorizontal);
				else if (hoveredType == 2) // End
					EditorGUIUtility.AddCursorRect(new Rect(r.xMax - 7, r.y, 14, r.height), MouseCursor.ResizeHorizontal);
				else // Body
					EditorGUIUtility.AddCursorRect(r, MouseCursor.MoveArrow);
			}

			// Dragging Logic
			switch(e.type)
			{
				case EventType.MouseDown:
					if(hoveredEvent != -1 && e.button == 0)
					{
						_isDragging = true;
						_draggedEventIndex = hoveredEvent;
						_selectedEventMap[path] = hoveredEvent;
						_dragType = hoveredType;

						var p = eventsProp.GetArrayElementAtIndex(_draggedEventIndex);
						var startProp = p?.FindPropertyRelative("StartTimeNormalized");
						var endProp = p?.FindPropertyRelative("EndTimeNormalized");

						if (startProp != null && endProp != null)
						{
							_dragInitialStartNormalized = startProp.floatValue;
							_dragInitialEndNormalized = endProp.floatValue;
							_dragOffsetNormalized = e.mousePosition.x / (frameWidth * totalEditorFrames);
							e.Use();
						}
						else
						{
							_isDragging = false;
							_draggedEventIndex = -1;
						}
					}
					else if(e.button == 0)
					{
						_selectedEventMap[path] = -1;
					}
					break;

				case EventType.MouseDrag:
					if(_isDragging && _draggedEventIndex != -1)
					{
						var currentNormalized = e.mousePosition.x / (frameWidth * totalEditorFrames);
						var delta = currentNormalized - _dragOffsetNormalized;

						var p = eventsProp.GetArrayElementAtIndex(_draggedEventIndex);
						var startProp = p?.FindPropertyRelative("StartTimeNormalized");
						var endProp = p?.FindPropertyRelative("EndTimeNormalized");

						if (startProp == null || endProp == null)
						{
							_isDragging = false;
							_draggedEventIndex = -1;
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
					_draggedEventIndex = -1;
					break;
			}
		}

		private void AddEvent(SerializedProperty eventsProp, int layerIndex, float startTime, float duration, System.Type type)
		{
			eventsProp.InsertArrayElementAtIndex(eventsProp.arraySize);
			var newEvent = eventsProp.GetArrayElementAtIndex(eventsProp.arraySize - 1);
			
			if (type != null)
			{
				newEvent.managedReferenceValue = System.Activator.CreateInstance(type);
			}

			var nameProp = newEvent.FindPropertyRelative("Name");
			var layerProp = newEvent.FindPropertyRelative("LayerIndex");
			var startProp = newEvent.FindPropertyRelative("StartTimeNormalized");
			var endProp = newEvent.FindPropertyRelative("EndTimeNormalized");

			if (nameProp != null) nameProp.stringValue = "New " + (type?.Name ?? "Event");
			if (layerProp != null) layerProp.intValue = layerIndex;
			if (startProp != null) startProp.floatValue = startTime;
			if (endProp != null) endProp.floatValue = startTime + duration;
			
			eventsProp.serializedObject.ApplyModifiedProperties();
		}
	}
}
