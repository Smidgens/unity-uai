// smidgens @ github

// ReSharper disable All

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

	internal sealed class NestedAssetList<T> where T : UAIScriptableObject
	{
		public delegate void ListItemDrawFn(ref Rect rect, SerializedProperty prop, T item);

		public ListItemDrawFn onDrawListItem = null;

		public bool IsIndexSelected(int index) => _assetList.IsSelected(index);
		
		public bool DrawTypeIcon { get; set; }

		public int Count => _assetList.count;

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
			_arrayProp = prop.FindPropertyRelative(nameof(SOArray<UAIScriptableObject>._array));
			_addContext = UAIEditorUtils.CreateTypeMenu(typeof(T), OnAddOption);
			_assetList = new ReorderableList(_arrayProp.serializedObject, _arrayProp);
			_assetList.onAddDropdownCallback = (r, l) => _addContext.DropDown(r);
			_assetList.onRemoveCallback = OnListRemove;

			_assetList.drawHeaderCallback = rect =>
			{
				EditorGUI.LabelField(rect, new  GUIContent(prop.displayName), EditorStyles.whiteLargeLabel);
			};

			_assetList.drawElementCallback = (rect, index, isActive, isFocused) =>
			{
				DrawListItem(rect, index);
			};
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
				Editor.DestroyImmediate(_childInspector);
				_childInspector = null;
			}
		}

		private ReorderableList _assetList = null;
		private SerializedProperty _arrayProp = null;
		private GenericMenu _addContext = null;
		private Editor _childInspector = null;
		private string _defaultIconGuidGuid = null;
		private Lazy<Texture> _defaultTypeIcon = null;

		private GUIContent _contextIcon;

		private void DrawContextButton(Rect rect, T asset)
		{
			if (_contextIcon == null)
			{
				_contextIcon = new GUIContent(EditorGUIUtility.FindTexture("_Menu"));
			}
			
			if (GUI.Button(rect, _contextIcon, EditorStyles.iconButton))
			{
				ShowContextMenu(asset);
			}
		}

		private void ShowContextMenu(T asset)
		{
			var scriptFile = UAIEditorUtils.GetObjectMonoscript(asset);
			var fileName = scriptFile.name;
			var m = new GenericMenu();
			m.AddItem(new GUIContent($"Edit Script"), false, () => AssetDatabase.OpenAsset(scriptFile));
			m.ShowAsContext();
		}

		private void OnAddOption(object option)
		{
			EditorApplication.delayCall += () => AddAsset(option as Type, _arrayProp);
		}

		private void DelayCall(Action action) => EditorApplication.delayCall += () => action.Invoke();

		private void EnsureInspector()
		{
			var i = _assetList.index;
			var currentArrItem = i >= 0 && i < _arrayProp.arraySize
			? _arrayProp.GetArrayElementAtIndex(i)
			: null;

			UnityEngine.Object currentItem = null;

			if (currentArrItem != null)
			{
				currentItem = currentArrItem.FindPropertyRelative(nameof(SORef<UAIScriptableObject>.item)).objectReferenceValue;
			}

			if (_childInspector && (_childInspector.target != currentItem || !_childInspector.target))
			{
				UnityEngine.Object.DestroyImmediate(_childInspector);
				_childInspector = null;
			}

			if (!_childInspector && currentItem)
			{
				_childInspector = Editor.CreateEditor(currentItem);
			}
		}

		private void DrawListItem(Rect rect, int index)
		{
			rect.SliceLeft(2f);
			rect.SliceRight(2f);
			
			SerializedProperty prop = _arrayProp.GetArrayElementAtIndex(index);
			SerializedProperty obProp = prop.FindPropertyRelative("item");

			var asset = obProp.objectReferenceValue as T;

			if (DrawTypeIcon)
			{
				var iconRect = rect.SliceLeft(rect.height);
				rect.SliceLeft(1);
				DrawIcon(iconRect, asset);
			}

			var ctxRect = rect.SliceRight(rect.height * 0.6f);
			rect.SliceRight(5f);
			DrawContextButton(ctxRect, asset);
			
			var checkRect = rect.SliceLeft(rect.height);
			var newEnabled = GUI.Toggle(checkRect, asset._enabled, GUIContent.none);
			if (newEnabled != asset._enabled)
			{
				Undo.RecordObject(asset, "Toggle enabled");
				asset._enabled = newEnabled;
			}

			if (asset && onDrawListItem != null)
			{
				var labelRect = rect;
				labelRect.height = EditorGUIUtility.singleLineHeight;
				labelRect.center = rect.center;
				onDrawListItem.Invoke(ref labelRect, prop, asset);

				EditorGUI.LabelField(labelRect, asset._label);
				return;
			}
			else
			{
				EditorGUI.LabelField(rect, asset?._label ?? "null");
			}
		}
		
		private void OnListRemove(ReorderableList list)
		{
			var i = list.index;
			var sp = list.serializedProperty;
			var arrItem = list.serializedProperty.GetArrayElementAtIndex(i);
			var obProp = arrItem.FindPropertyRelative(nameof(SORef<UAIScriptableObject>.item));
			var idProp = arrItem.FindPropertyRelative(nameof(SORef<UAIScriptableObject>.id));
			var asset = obProp.objectReferenceValue as UAIScriptableObject;
			var path = AssetDatabase.GetAssetPath(asset);

			var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
			sp.DeleteArrayElementAtIndex(i);
			sp.serializedObject.ApplyModifiedProperties();

			List<UAIScriptableObject> destroyList = new();
			destroyList.Add(asset);
			asset.GatherNestedAssets(destroyList);

			destroyList.ForEach(Undo.DestroyObjectImmediate);

		}

		private static string GetDefaultAssetName(Type type)
		{
			if (type == null)
			{
				return "";
			}

			var displayName = type.GetCustomAttribute<DisplayNameAttribute>();

			if (displayName != null)
			{
				var startIndex = displayName.DisplayName.LastIndexOf("/");
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
			
			var mainAsset = arrayProp.serializedObject.targetObject as UnityEngine.Object;
			var newAsset = ScriptableObject.CreateInstance(assetType) as UAIScriptableObject;
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

			// arrItem.objectReferenceValue = newAsset;
			arrayProp.serializedObject.ApplyModifiedProperties();
		}

		private void DrawIcon(Rect rect, ScriptableObject asset)
		{
			rect.Resize(-2f);
			var ms = MonoScript.FromScriptableObject(asset);
			var path = AssetDatabase.GetAssetPath(ms);
			Texture ico = AssetDatabase.GetCachedIcon(path);

			if (_defaultTypeIcon != null && UAIEditorUtils.IsDefaultScriptIcon(ico))
			{
				ico = _defaultTypeIcon.Value;
			}

			if (!ico)
			{
				return;
			}
			GUI.DrawTexture(rect, ico, ScaleMode.StretchToFill);
		}

		private static void DrawIconBasic(Rect rect, ScriptableObject asset)
		{
			rect.Resize(-2f);
			
			var ms = MonoScript.FromScriptableObject(asset);
			var path = AssetDatabase.GetAssetPath(ms);
			Texture ico = AssetDatabase.GetCachedIcon(path);

			if (!ico)
			{
				return;
			}
			GUI.DrawTexture(rect, ico, ScaleMode.StretchToFill);
		}
		
		// private void DrawScriptInfo()
		// {
		// 	if (!_childInspector)
		// 	{
		// 		return;
		// 	}
		//
		// 	var type = _childInspector.target.GetType();
		//
		// 	var typeLabel = $"{type.Assembly.GetName().Name}.{type.Name}";
		//
		// 	var open = false;
		//
		// 	var tempColor = GUI.backgroundColor;
		// 	GUI.backgroundColor = Color.cyan * 0.5f;
		// 	
		// 	EditorGUILayout.BeginVertical(GUI.skin.box);
		//
		// 	var btnLabel = new GUIContent("Edit");
		// 	var btnStyle = EditorStyles.miniButton;
		// 	var btnWidth = btnStyle.CalcSize(btnLabel).x;
		// 	
		// 	var dRect = EditorGUILayout.GetControlRect(GUILayout.Height(EditorGUIUtility.singleLineHeight));
		// 	dRect.SliceLeft(2f);
		//
		// 	var iconRect = dRect.SliceLeft(dRect.height);
		// 	dRect.SliceLeft(2f);
		// 	
		// 	var btnRect = dRect.SliceRight(btnWidth);
		// 	dRect.SliceRight(2f);
		//
		// 	DrawIconBasic(iconRect, _childInspector.target as ScriptableObject);
		// 	
		// 	EditorGUI.LabelField(dRect, typeLabel, EditorStyles.miniLabel);
		//
		// 	open = GUI.Button(btnRect, btnLabel, btnStyle);
		//
		// 	EditorGUILayout.EndVertical();
		//
		// 	GUI.backgroundColor = tempColor;
		//
		// 	if (open)
		// 	{
		// 		UtilityEditorUtils.OpenScriptEditor(_childInspector.target);
		// 	}
		// }

	}
	
	
}

#endif