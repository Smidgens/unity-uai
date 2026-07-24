// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Consideration List")]
	public sealed class UAIConsiderationList : ScriptableObject
	{
		[HideInInspector]
		[SerializeField] internal SOArray<UAIConsideration> _considerations = new();

		internal int GetEnabledConsiderationCount()
		{
			int count = 0;
			foreach(var c in _considerations.GetArr())
			{
				if (c.item && c.item._enabled)
				{
					count++;
				}
			}
			return count;
		}
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using UObject = UnityEngine.Object;
	using SP = UnityEditor.SerializedProperty;
	using RL = UnityEditorInternal.ReorderableList;

	[CustomEditor(typeof(UAIConsiderationList))]
	internal class _UtilityConsiderationSetSO : _UAIScriptableObject
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			EditorGUILayout.Space(5f);
			serializedObject.ApplyModifiedProperties();
			EditorGUILayout.Space(5f);
			_considerationAssetList.OnListGUI();
			serializedObject.ApplyModifiedProperties();
		}

		private NestedAssetList<UAIConsideration> _considerationAssetList = null;

		private void OnEnable()
		{
			var listProp = serializedObject.FindProperty(nameof(UAIConsiderationList._considerations));
			_considerationAssetList = CreateConsiderationList(listProp);
		}

		private void OnDisable()
		{
			_considerationAssetList?.DisposeGUI();
		}
		
		private static NestedAssetList<UAIConsideration> CreateConsiderationList(SerializedProperty listProp)
		{
			NestedAssetList<UAIConsideration> view = new (listProp);
			view.DefaultTypeIconGUID = UAIConstants.DEFAULT_CONSIDERATION_ICON_GUID;
			view.DrawTypeIcon = true;
			view.onDrawListItem = (ref Rect rect, SerializedProperty prop, UAIConsideration so) =>
			{
				if (!so)
				{
					return;
				}
				var curveRect = rect.SliceRight(rect.height * 1.5f);
				EditorGUI.BeginChangeCheck();
				var changedCurve = EditorGUI.CurveField(curveRect, new AnimationCurve(so._curve.keys));
				if (EditorGUI.EndChangeCheck())
				{
					Undo.RecordObject(so, "Change curve");
					so._curve = changedCurve;
				}
				
				var invertRect = rect.SliceRight(60f);
				var newInvert = EditorGUI.ToggleLeft(invertRect, new GUIContent("Invert"), so._invert);
				if (newInvert != so._invert)
				{
					Undo.RecordObject(so, "Toggle inverted");
					so._invert = newInvert;
				}
			};
			return view;
		}


	}
}

#endif