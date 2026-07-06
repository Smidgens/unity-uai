// smidgens @ github

// ReSharper disable All

// #if UNITY_EDITOR
//
// namespace Smidgenomics.Unity.UtilityAI.Editor
// {
// 	using UnityEngine;
// 	using UnityEditor;
// 	using System;
// 	using System.Linq;
// 	using System.Collections.Generic;
// 	using UObject = UnityEngine.Object;
// 	using SP = UnityEditor.SerializedProperty;
// 	using RL = UnityEditorInternal.ReorderableList;
//
// 	[CustomEditor(typeof(UtilityAIAction), true)]
// 	internal class _UtilityAIAction : Editor
// 	{
// 		public override void OnInspectorGUI()
// 		{
// 			serializedObject.UpdateIfRequiredOrScript();
//
// 			EditorGUILayout.BeginVertical(GUI.skin.box);
// 			foreach (var prop in _props)
// 			{
// 				if (prop == null)
// 				{
// 					continue;
// 				}
// 				EditorGUILayout.PropertyField(prop);
// 			}
// 			EditorGUILayout.EndVertical();
// 			serializedObject.ApplyModifiedProperties();
// 			EditorGUILayout.Space();
// 			_considerationView.OnListGUI();
// 			serializedObject.ApplyModifiedProperties();
// 		}
//
// 		private NestedAssetList<UtilityAIConsideration> _considerationView = null;
// 		private List<SP> _props = new();
//
// 		private void OnEnable()
// 		{
// 			_props = new List<SP>();
// 			foreach (var f in target.GetType().FindInspectorFields())
// 			{
// 				var prop = serializedObject.FindProperty(f.Name);
// 				_props.Add(prop);
// 			}
// 			var listProp = serializedObject.FindProperty(nameof(UtilityAIAction._considerations));
// 			_considerationView = CreateConsiderationList(listProp);
// 		}
//
// 		private static NestedAssetList<UtilityAIConsideration> CreateConsiderationList(SerializedProperty listProp)
// 		{
// 			NestedAssetList<UtilityAIConsideration> view = new (listProp);
// 			view.DefaultTypeIconGUID = "d8ec218438d247b49a3a0f61ed39664d";
// 			view.DrawTypeIcon = true;
// 			view.onDrawListItem = (ref Rect rect, SerializedProperty prop, UtilityAIConsideration so) =>
// 			{
// 				if (!so)
// 				{
// 					return;
// 				}
// 				var curveRect = rect.SliceRight(rect.height * 1.5f);
// 				EditorGUI.BeginChangeCheck();
// 				var changedCurve = EditorGUI.CurveField(curveRect, new AnimationCurve(so._curve.keys));
// 				if (EditorGUI.EndChangeCheck())
// 				{
// 					Undo.RecordObject(so, "Change curve");
// 					so._curve = changedCurve;
// 				}
// 				
// 				var invertRect = rect.SliceRight(60f);
// 				var newInvert = EditorGUI.ToggleLeft(invertRect, new GUIContent("Invert"), so._invert);
// 				if (newInvert != so._invert)
// 				{
// 					Undo.RecordObject(so, "Toggle inverted");
// 					so._invert = newInvert;
// 				}
// 			};
// 			return view;
// 		}
//
// 		private void OnDisable()
// 		{
// 			if (_considerationView != null)
// 			{
// 				_considerationView.DisposeGUI();
// 			}
// 		}
// 		
// 	}
// }
//
// #endif