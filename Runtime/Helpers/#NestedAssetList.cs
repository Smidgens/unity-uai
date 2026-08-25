// smidgens @ github

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Reflection;
	using UnityEditorInternal;
	using Object = UnityEngine.Object;

	internal sealed class NestedAssetList<T> where T : UAIScriptableObject
	{
		public delegate void ListItemDrawFn(ref Rect rect, SerializedProperty prop, T item);
		public delegate void HeaderCallback(Rect rect);

		public ListItemDrawFn onDrawListItem = null;
		public HeaderCallback onDrawHeader;
		public HeaderCallback onDrawNone;

		public bool IsIndexSelected(int index) => _assetList.IsSelected(index);
		
		public bool DrawTypeIcon { get; set; }

		public float HeaderHeight
		{
			get => _assetList.headerHeight;
			set => _assetList.headerHeight = value;
		}

		public int Count => _assetList.count;
		
		public string HeaderLabel { get; set; }

		public string DefaultTypeIconGUID
		{
			get => _defaultIconGuidGuid;
			set
			{
				_defaultIconGuidGuid = value;
				if (_defaultIconGuidGuid == null)
				{
					_defaultTypeIcon = null;
					return;
				}
				var iconGuid = value;
				_defaultTypeIcon = new Lazy<Texture>(() =>
				{
					return AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(iconGuid));
				});
			}
		}

		public NestedAssetList(SerializedProperty prop)
		{
			_outerProp = prop;
			_arrayProp = prop.FindPropertyRelative(nameof(SOArray<UAIScriptableObject>._array));
			_addContext = UAIEditorUtils.CreateTypeMenu(typeof(T), OnAddOption, null);
			_assetList = new ReorderableList(_arrayProp.serializedObject, _arrayProp)
			{
				drawHeaderCallback = DrawHeader,
				drawElementCallback = DrawListItem,
				displayAdd = false,
				displayRemove = false,
				draggable = true,
				elementHeightCallback = i => EditorGUIUtility.singleLineHeight
			};
			_assetList.drawNoneElementCallback = rect =>
			{
				if (onDrawNone != null)
				{
					onDrawNone.Invoke(rect);
				}
				else
				{
					GUI.Label(rect, "List is Empty");
				}
			};
			_assetList.footerHeight = 0f;
		}

		// 
		public void OnListGUI()
		{
			if (_assetList == null)
			{
				return;
			}

			_assetList.serializedProperty.serializedObject.UpdateIfRequiredOrScript();
			_assetList.DoLayoutList();
			_assetList.serializedProperty.serializedObject.ApplyModifiedProperties();

			EnsureInspector();

			if (_childInspector && _childInspector.target)
			{
				EditorGUILayout.Space(2);
				_childInspector.OnInspectorGUI();
			}
		}

		public void DisposeGUI()
		{
			if (_childInspector)
			{
				Object.DestroyImmediate(_childInspector);
				_childInspector = null;
			}
		}

		private readonly ReorderableList _assetList;
		private readonly SerializedProperty _outerProp;
		private readonly SerializedProperty _arrayProp;
		private Editor _childInspector;
		private string _defaultIconGuidGuid;
		private Lazy<Texture> _defaultTypeIcon;
		private GUIContent _contextIcon;
		private float _ctxButtonHeight;
		private readonly GenericMenu _addContext;
		private readonly GUIContent _addBtn = EditorGUIUtility.IconContent("Toolbar Plus More");

		private Lazy<GUIStyle> _headerLabelStyle = new(() =>
		{
			var color = EditorStyles.largeLabel.normal.textColor;

			if (EditorGUIUtility.isProSkin)
			{
				color = Color.white * 0.9f;
			}
			
			var s = new GUIStyle(EditorStyles.largeLabel)
			{
				fontStyle = FontStyle.Bold,
				normal =
				{
					textColor = color
				}
			};
			return s;
		});

		private string GetDisplayName()
		{
			if (!string.IsNullOrEmpty(HeaderLabel))
			{
				return HeaderLabel;
			}
			return _outerProp.displayName;
		}


		private void DrawHeader(Rect rect)
		{
			var btnSize = EditorStyles.iconButton.CalcSize(_addBtn);

			var btnRect = rect.SliceRight(btnSize.x);
			var btnCenter = btnRect.center;

			btnRect.height = btnSize.y;
			btnRect.center = btnCenter;

			if (onDrawHeader != null)
			{
				onDrawHeader.Invoke(rect);
			}
			else
			{
				EditorGUI.LabelField(rect, GetDisplayName(), _headerLabelStyle.Value);
			}

			if (GUI.Button(btnRect, _addBtn, EditorStyles.iconButton))
			{
				_addContext.DropDown(btnRect);
			}
			
		}

		private static readonly GUIContent _LB_EDIT_SCRIPT = new ("Edit Script");

		private void DrawContextButton(Rect rect, T asset, int index)
		{
			if (_contextIcon == null)
			{
				_contextIcon = EditorGUIUtility.IconContent("_Menu");
				_ctxButtonHeight = EditorStyles.iconButton.CalcHeight(_contextIcon, 100);
			}

			var btnRect = rect;
			var btnCenter = btnRect.center;
			btnRect.height = _ctxButtonHeight;
			btnRect.center = btnCenter;

			if (GUI.Button(btnRect, _contextIcon, EditorStyles.iconButton))
			{
				var m = new GenericMenu();

				if (CanEditScript(asset.GetType()))
				{
					m.AddItem(_LB_EDIT_SCRIPT, false, () => UAIEditorUtils.OpenScriptEditor(asset));
				}
				else
				{
					m.AddDisabledItem(_LB_EDIT_SCRIPT);
				}
				m.AddSeparator(string.Empty);
				m.AddItem(new GUIContent("Remove"), false, () => RemoveAtIndex(index));
				m.ShowAsContext();
			}
		}

		private static bool CanEditScript(Type type)
		{
			// TODO: Only return false if script was installed via package manager
			if (type.Assembly == typeof(UAIAction).Assembly)
			{
				#if SM_DEV
				return true;
				#else
				return false;
				#endif
			}
			return true;
		}

		private void RemoveAtIndex(int i)
		{
			var arrItem = _assetList.serializedProperty.GetArrayElementAtIndex(i);
			var obProp = arrItem.FindPropertyRelative(nameof(SORef<UAIScriptableObject>.item));
			var asset = obProp.objectReferenceValue as UAIScriptableObject;
			_assetList.serializedProperty.DeleteArrayElementAtIndex(i);
			_assetList.serializedProperty.serializedObject.ApplyModifiedProperties();
			if (asset)
			{
				List<UAIScriptableObject> destroyList = new() { asset };
				asset.GatherNestedAssets(destroyList);
				destroyList.ForEach(Undo.DestroyObjectImmediate);
			}
		}

		private void OnAddOption(object option)
		{
			EditorApplication.delayCall += () => AddAsset(option as Type, _arrayProp);
		}

		private void EnsureInspector()
		{
			var i = _assetList.index;
			var currentArrItem = i >= 0 && i < _arrayProp.arraySize
			? _arrayProp.GetArrayElementAtIndex(i)
			: null;

			Object currentItem = null;

			if (currentArrItem != null)
			{
				currentItem = currentArrItem.FindPropertyRelative(nameof(SORef<UAIScriptableObject>.item)).objectReferenceValue;
			}

			if (_childInspector && (_childInspector.target != currentItem || !_childInspector.target))
			{
				Object.DestroyImmediate(_childInspector);
				_childInspector = null;
			}

			if (!_childInspector && currentItem)
			{
				_childInspector = Editor.CreateEditor(currentItem);
			}
		}

		private void DrawListItem(Rect rect, int index, bool active, bool focused)
		{
			SerializedProperty prop = _arrayProp.GetArrayElementAtIndex(index);
			SerializedProperty obProp = prop.FindPropertyRelative("item");

			var asset = obProp.objectReferenceValue as T;

			if (!asset)
			{
				EditorGUI.LabelField(rect, "null");
				return;
			}

			if (DrawTypeIcon)
			{
				var iconRect = rect.SliceLeft(rect.height);
				rect.SliceLeft(EditorGUIUtility.standardVerticalSpacing);
				DrawIcon(iconRect, asset);
			}

			var ctxRect = rect.SliceRight(rect.height * 0.6f);
			rect.SliceRight(EditorGUIUtility.standardVerticalSpacing);
			DrawContextButton(ctxRect, asset, index);

			var checkRect = rect.SliceLeft(rect.height);
			var newEnabled = GUI.Toggle(checkRect, asset._enabled, GUIContent.none);
			if (newEnabled != asset._enabled)
			{
				EditorApplication.delayCall += () =>
				{
					Undo.RecordObject(asset, "Toggle enabled");
					asset._enabled = newEnabled;
				};
			}
			var labelRect = rect;
			labelRect.height = EditorGUIUtility.singleLineHeight;
			labelRect.center = rect.center;

			if (onDrawListItem != null)
			{
				onDrawListItem.Invoke(ref labelRect, prop, asset);
			}

			labelRect.SliceRight(EditorGUIUtility.standardVerticalSpacing);
			
			DoItemLabel(labelRect, index, asset, focused);
		}

		private int _labelEditIndex = -1;
		private (int, double) _lastMouseDown;
		private const double _DOUBLE_CLICK_THRESHOLD = 0.2;
		private const string _LABEL_CONTROL_NAME = "asset_label";

		private bool _didRename;

		private void DoItemLabel(Rect rect, int i, T asset, bool focused)
		{
			// Note: this is a mess

			if (_labelEditIndex != i)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					if (rect.Contains(Event.current.mousePosition))
					{
						var (lastIndex, lastTime) = _lastMouseDown;
						var elapsed = EditorApplication.timeSinceStartup - lastTime;
						if (lastIndex == i && elapsed < _DOUBLE_CLICK_THRESHOLD)
						{
							_labelEditIndex = i;
						}
						_lastMouseDown = (i, EditorApplication.timeSinceStartup);
					}
				}
				else if (focused && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F2)
				{
					_labelEditIndex = i;
					_didRename = true;
					Event.current.Use();
				}
			}
			if (_labelEditIndex == i)
			{
				if (_assetList.index != i)
				{
					_labelEditIndex = -1;
				}
				else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
				{
					_labelEditIndex = -1;
					Event.current.Use();
				}
				else
				{
					if (_didRename)
					{
						GUI.FocusControl(_LABEL_CONTROL_NAME);
						EditorGUI.FocusTextInControl(_LABEL_CONTROL_NAME);
						_didRename = false;
					}
					EditorGUI.BeginChangeCheck();
					GUI.SetNextControlName(_LABEL_CONTROL_NAME);
					var newLabel = EditorGUI.DelayedTextField(rect, asset._label);

					
					if (EditorGUI.EndChangeCheck())
					{
						_labelEditIndex = -1;
						if (newLabel.Length > 0)
						{
							Undo.RecordObject(asset, "Edit label");
							asset._label = newLabel;
						}
				
					}
				}
			}
			else
			{
				EditorGUI.LabelField(rect, asset._label);
			}
		}

		private static string GetDefaultAssetName(Type type)
		{
			if (type == null)
			{
				return string.Empty;
			}

			var displayName = type.GetCustomAttribute<DisplayNameAttribute>();

			if (displayName != null)
			{
				var startIndex = displayName.DisplayName.LastIndexOf('/');
				if (startIndex < 0)
				{
					startIndex = 0;
				}
				else
				{
					startIndex++;
				}

				return displayName.DisplayName.Substring(startIndex);
			}
			
			

			return type.Name;
		} 

		// adds a new SO asset of given type to main asset and inserts it to array
		private void AddAsset(Type assetType, SerializedProperty arrayProp)
		{
			if (assetType == null)
			{
				return;
			}
			
			var mainAsset = arrayProp.serializedObject.targetObject;
			var newAsset = (ScriptableObject.CreateInstance(assetType) as UAIScriptableObject)!;
			newAsset.hideFlags = HideFlags.HideInHierarchy;

			var assetName = GetDefaultAssetName(assetType);
			
			newAsset.name = assetName;
			newAsset._label = assetName;
			Undo.RegisterCreatedObjectUndo(newAsset, "Create child asset");
			AssetDatabase.AddObjectToAsset(newAsset, mainAsset);
			var newIndex = arrayProp.arraySize;
			arrayProp.InsertArrayElementAtIndex(newIndex);

			var arrItem = arrayProp.GetArrayElementAtIndex(newIndex);
			var obProp = arrItem.FindPropertyRelative(nameof(SORef<UAIScriptableObject>.item));
			var idProp = arrItem.FindPropertyRelative(nameof(SORef<UAIScriptableObject>.id));
			idProp.stringValue = newAsset._id;
			obProp.objectReferenceValue = newAsset;
			arrayProp.serializedObject.ApplyModifiedProperties();

			// _assetList.index = _assetList.count - 1;
			_assetList.Select(_assetList.count - 1);
			_assetList.GrabKeyboardFocus();

		}

		private void DrawIcon(Rect rect, T asset)
		{
			rect.Resize(-rect.height * 0.15f);
			var c = Color.white;
			if (!asset._enabled)
			{
				c.a = 0.5f;
			}
			UAIEditorGUI.DrawTypeIcon(rect, asset, c);
		}

	}
	
	
}

#endif