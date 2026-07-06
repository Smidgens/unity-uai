// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.Linq;

	[System.Serializable]
	internal struct SORef<T> where T : UtilityAISO
	{
		public T item;
		// saved id in case ref gets lost
		public string id;
	}

	[System.Serializable]
	internal sealed class SOArray<T> where T : UtilityAISO
	{
		public T[] GetItems()
		{
			return _array.Select(x => x.item).Where(x => x != null).ToArray();
		}

		public ref SORef<T>[] GetArr()
		{
			return ref _array;
		}

		[SerializeField] internal SORef<T>[] _array = { };

		[HideInInspector]
		[SerializeField] internal int _selectedIndex = -1;
	}
}

namespace Smidgenomics.Unity.UAI
{
	using System;

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class SOArrayColumn : Attribute
	{
		public float width { get; }
		public bool label { get; }

		public SOArrayColumn(float width = 0, bool label = false)
		{
			this.width = width;
			this.label = label;
		}
	}
}


#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;
	using SP = UnityEditor.SerializedProperty;

	// [CustomPropertyDrawer(typeof(SOArray<>))]
	internal sealed class _SOArray : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SP prop, GUIContent l)
		{
			if (!prop.serializedObject.targetObject.GetType().IsSubclassOf(typeof(ScriptableObject)))
			{
				return;
			}

			if (fieldInfo.FieldType.IsArray)
			{
				return;
			}

			if(l != GUIContent.none && !fieldInfo.FieldType.IsArray)
			{
				// pos = EditorGUI.PrefixLabel(pos, l);
			}

			pos.SliceTop(PAD);
			pos.SliceBottom(PAD);

			using (new EditorGUI.PropertyScope(pos, l, prop))
			{
				_cachedList.DoList(pos);
			}
		}

		public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			if (_cachedList == null || _cachedList.serializedProperty.serializedObject != prop.serializedObject)
			{
				var indexProp = prop.FindPropertyRelative("_selectedIndex");
				var arrProp = prop.FindPropertyRelative(nameof(SOArray<UtilityAISO>._array));

				_cachedList = new ReorderableList(prop.serializedObject, arrProp);
				_cachedList.drawHeaderCallback = pos =>
				{
					EditorGUI.LabelField(pos, prop.displayName, EditorStyles.whiteLargeLabel);
				};
				_cachedList.onAddDropdownCallback = OnListAdd;

				_cachedList.onRemoveCallback = OnListRemove;

				_cachedList.drawElementCallback = (rect, index, active, focused) =>
				{
					var item = _cachedList.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(rect, item, GUIContent.none);
				};

				_cachedList.onSelectCallback = list =>
				{
					indexProp.intValue = list.index;
					prop.serializedObject.ApplyModifiedPropertiesWithoutUndo();
				};
			}

			return _cachedList.GetHeight() + PAD * 2;
		}

		private const float PAD = 4;
		private ReorderableList _cachedList = null;

		private void OnListAdd(Rect pos, ReorderableList list)
		{
			var genericType = fieldInfo.FieldType.GenericTypeArguments[0];
			
			var m = UtilityEditorUtils.CreateTypeMenu(genericType, o =>
			{
				AddAsset((Type)o, list.serializedProperty);
			});
			
			m.DropDown(pos);
		}

		private static void AddAsset(Type assetType, SerializedProperty arrayProp)
		{
			var mainAsset = arrayProp.serializedObject.targetObject as UnityEngine.Object;
			var newAsset = ScriptableObject.CreateInstance(assetType) as UtilityAISO;
			newAsset.hideFlags = HideFlags.HideInHierarchy;
			newAsset.name = assetType.Name;
			Undo.RegisterCreatedObjectUndo(newAsset, "Create child asset");
			AssetDatabase.AddObjectToAsset(newAsset, mainAsset);
			var newIndex = arrayProp.arraySize;
			arrayProp.InsertArrayElementAtIndex(newIndex);

			var arrItem = arrayProp.GetArrayElementAtIndex(newIndex);
			var obProp = arrItem.FindPropertyRelative(nameof(SORef<UtilityAISO>.item));
			var idProp = arrItem.FindPropertyRelative(nameof(SORef<UtilityAISO>.id));

			idProp.stringValue = newAsset._id;
			obProp.objectReferenceValue = newAsset;

			// arrItem.objectReferenceValue = newAsset;
			arrayProp.serializedObject.ApplyModifiedProperties();
		}
		
		private void OnListRemove(ReorderableList list)
		{
			var i = list.index;
			var sp = list.serializedProperty;
			var arrItem = list.serializedProperty.GetArrayElementAtIndex(i);
			var obProp = arrItem.FindPropertyRelative(nameof(SORef<UtilityAISO>.item));
			var idProp = arrItem.FindPropertyRelative(nameof(SORef<UtilityAISO>.id));
			var asset = obProp.objectReferenceValue as UtilityAISO;
			
			var path = AssetDatabase.GetAssetPath(asset);
			var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
			sp.DeleteArrayElementAtIndex(i);
			sp.serializedObject.ApplyModifiedProperties();
			Undo.DestroyObjectImmediate(asset);
		}

	}

}

#endif

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;
	using SP = UnityEditor.SerializedProperty;

	[CustomPropertyDrawer(typeof(SORef<>))]
	internal sealed class _SORef : PropertyDrawer
	{
		public override void OnGUI(Rect pos, SP prop, GUIContent l)
		{
			using (new EditorGUI.PropertyScope(pos, l, prop))
			{

			}
		}

		// public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		// {
		// }


	}

}

#endif