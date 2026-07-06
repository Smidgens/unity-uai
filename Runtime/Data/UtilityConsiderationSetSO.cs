// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Consideration Set")]
	public sealed class UtilityConsiderationSetSO : ScriptableObject
	{
		[HideInInspector]
		[SerializeField] internal SOArray<UtilityAIConsideration> _considerations = new();

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

	[CustomEditor(typeof(UtilityConsiderationSetSO))]
	internal class _UtilityConsiderationSetSO : _UtilityAISO
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

		private NestedAssetList<UtilityAIConsideration> _considerationAssetList = null;

		private void OnEnable()
		{
			var listProp = serializedObject.FindProperty(nameof(UtilityConsiderationSetSO._considerations));
			_considerationAssetList = CreateConsiderationList(listProp);
		}

		private void OnDisable()
		{
			_considerationAssetList?.DisposeGUI();
		}
		
		private static NestedAssetList<UtilityAIConsideration> CreateConsiderationList(SerializedProperty listProp)
		{
			NestedAssetList<UtilityAIConsideration> view = new (listProp);
			view.DefaultTypeIconGUID = "d8ec218438d247b49a3a0f61ed39664d";
			view.DrawTypeIcon = true;
			view.onDrawListItem = (ref Rect rect, SerializedProperty prop, UtilityAIConsideration so) =>
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