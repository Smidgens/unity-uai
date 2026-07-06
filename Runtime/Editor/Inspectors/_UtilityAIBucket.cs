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
// 	[CustomEditor(typeof(UtilityAIBucket))]
// 	internal class _UtilityAIBucket : _UtilityAISO
// 	{
// 		public override void OnInspectorGUI()
// 		{
// 			serializedObject.UpdateIfRequiredOrScript();
// 			EditorGUILayout.Space(5f);
// 			foreach (var prop in _props)
// 			{
// 				EditorGUILayout.PropertyField(prop);
// 			}
// 			serializedObject.ApplyModifiedProperties();
// 			EditorGUILayout.Space(5f);
// 			_actionAssetList.OnListGUI();
// 			serializedObject.ApplyModifiedProperties();
// 		}
//
// 		private NestedAssetList<UtilityAIAction> _actionAssetList = null;
// 		private IEnumerable<SerializedProperty> _props = null;
//
// 		private static string[] _extraFields =
// 		{
// 			nameof(UtilityAIBucket._label),
// 			nameof(UtilityAIBucket._comment),
// 			nameof(UtilityAIBucket._weight),
// 			nameof(UtilityAIBucket._actionSelector),
// 			nameof(UtilityAIBucket._actionScoringRate),
// 			nameof(UtilityAIBucket._bucketScoringRate),
// 		};
//
// 		private void OnEnable()
// 		{
// 			var listFields = DrawAssetListAttribute.FindFieldsForType(target.GetType());
//
// 			_props = _extraFields.Select(f => serializedObject.FindProperty(f)).Where(x => x != null);
// 			
// 			var listProp = serializedObject.FindProperty(nameof(UtilityAIBucket._actions));
// 			_actionAssetList = new NestedAssetList<UtilityAIAction>(listProp);
//
// 			_actionAssetList.DefaultTypeIconGUID = "b403041b6ec9a3744b4e92bc8014f7f6";
// 			_actionAssetList.DrawTypeIcon = true;
//
// 			_actionAssetList.onDrawListItem = (ref Rect rect, SerializedProperty prop, UtilityAIAction so) =>
// 			{
// 				var wrect = rect.SliceRight(50);
// 				var newWeight = Mathf.Max(EditorGUI.FloatField(wrect, so._weight), 0f);
// 				if (newWeight != so._weight)
// 				{
// 					Undo.RecordObject(so, "Change weight");
// 					so._weight = newWeight;
// 				}
// 			};
// 		}
//
// 		private void OnDisable()
// 		{
// 			// cleanup
// 			if (_actionAssetList != null)
// 			{
// 				_actionAssetList.DisposeGUI();
// 			}
// 		}
//
// 		private static string StringifyFloat(float v)
// 		{
// 			return v.ToString("0.0");
// 		}
//
//
// 	}
// }
//
// #endif