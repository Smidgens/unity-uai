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
// 	[CustomEditor(typeof(UtilityConsiderationSetSO))]
// 	internal class _UtilityConsiderationSetSO : Editor
// 	{
// 		public override void OnInspectorGUI()
// 		{
// 			serializedObject.UpdateIfRequiredOrScript();
// 			EditorGUILayout.Space(5f);
// 			// foreach (var prop in _props)
// 			// {
// 			// 	EditorGUILayout.PropertyField(prop);
// 			// }
// 			serializedObject.ApplyModifiedProperties();
// 			
// 			EditorGUILayout.Space(5f);
// 			_considerationAssetList.OnListGUI();
// 			serializedObject.ApplyModifiedProperties();
// 		}
//
// 		protected override bool ShouldHideOpenButton()
// 		{
// 			return true;
// 		}
//
// 		private NestedAssetList<UtilityAIConsideration> _considerationAssetList = null;
//
// 		private static string[] _extraFields =
// 		{
// 			
// 		};
//
// 		private void OnEnable()
// 		{
// 			var listProp = serializedObject.FindProperty(nameof(UtilityConsiderationSetSO._considerations));
// 			_considerationAssetList = new NestedAssetList<UtilityAIConsideration>(listProp);
//
// 			_considerationAssetList.DefaultTypeIconGUID = "b403041b6ec9a3744b4e92bc8014f7f6";
// 			_considerationAssetList.DrawTypeIcon = true;
//
// 			_considerationAssetList.onDrawListItem = (ref Rect rect, SerializedProperty prop, UtilityAIConsideration so) =>
// 			{
// 				if (!so)
// 				{
// 					return;
// 				}
// 				
// 				var checkRect = rect.SliceLeft(rect.height);
// 				var newEnabled = GUI.Toggle(checkRect, so._enabled, GUIContent.none);
// 				if (newEnabled != so._enabled)
// 				{
// 					Undo.RecordObject(so, "Toggle enabled");
// 					so._enabled = newEnabled;
// 				}
//
// 				var curveRect = rect.SliceRight(rect.height * 1.5f);
// 				var tenabled = GUI.enabled;
// 				// GUI.enabled = false;
//
// 				EditorGUI.BeginChangeCheck();
//
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
//
// 				GUI.enabled = tenabled;
// 				EditorGUI.LabelField(rect, so.name);
// 			};
// 		}
//
// 		private void OnDisable()
// 		{
// 			// cleanup
// 			if (_considerationAssetList != null)
// 			{
// 				_considerationAssetList.DisposeGUI();
// 			}
// 		}
//
// 		private static string StringifyFloat(float v)
// 		{
// 			return v.ToString("0.0");
// 		}
//
// 		
//
// 	}
// }
//
// #endif