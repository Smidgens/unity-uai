// smidgens @ github

// resharper disable all


namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.Collections.Generic;
	using UnityEngine.Serialization;

	/// <summary>
	/// Houses list of actions
	/// </summary>
	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Bucket")]
	[ExcludeFromPreset]
	[DrawAssetList("_actions")]
	[DrawAssetList("_considerations")]
	public sealed class UtilityAIBucket : UtilityAISO
	{
		public string BucketName => _label;

		[Multiline(2)]
		[SerializeField] internal string _comment = "";

		[Min(0)]
		[SerializeField] internal float _weight = 1;
		
		[Min(0.1f)]
		[SerializeField] internal float _actionScoringRate = 1f;

		[Min(0.1f)]
		[SerializeField] internal float _bucketScoringRate = 5f;

		[InstancedReference]
		[SerializeReference]
		internal UtilityAISelector _actionSelector = new UtilityAISelector_TopScore();
		
		[SerializeField] internal SOArray<UtilityAIAction> _actions = new();

		[SerializeField] internal SOArray<UtilityAIConsideration> _bucketConsiderations = new();

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

	[CustomEditor(typeof(UtilityAIBucket))]
	internal class _UtilityAIBucket : _UtilityAISO
	{
		public override void OnInspectorGUI()
		{
			serializedObject.UpdateIfRequiredOrScript();
			EditorGUILayout.Space(5f);
			foreach (var prop in _props)
			{
				EditorGUILayout.PropertyField(prop);
			}
			serializedObject.ApplyModifiedProperties();
			
			EditorGUILayout.Space(5f);
			_bucketConsiderations.OnListGUI();
			serializedObject.ApplyModifiedProperties();
			
			EditorGUILayout.Space(5f);
			_actionAssetList.OnListGUI();
			serializedObject.ApplyModifiedProperties();
		}

		private NestedAssetList<UtilityAIAction> _actionAssetList = null;
		private NestedAssetList<UtilityAIConsideration> _bucketConsiderations = null;
		private IEnumerable<SerializedProperty> _props = null;

		private static string[] _extraFields =
		{
			nameof(UtilityAIBucket._label),
			nameof(UtilityAIBucket._comment),
			nameof(UtilityAIBucket._weight),
			nameof(UtilityAIBucket._actionSelector),
			nameof(UtilityAIBucket._actionScoringRate),
			nameof(UtilityAIBucket._bucketScoringRate),
		};

		private void OnEnable()
		{
			var listFields = DrawAssetListAttribute.FindFieldsForType(target.GetType());
			_props = _extraFields.Select(f => serializedObject.FindProperty(f)).Where(x => x != null);
			_actionAssetList = CreateActionList(serializedObject.FindProperty(nameof(UtilityAIBucket._actions)));
			_bucketConsiderations = CreateConsiderationList(serializedObject.FindProperty(nameof(UtilityAIBucket._bucketConsiderations)));
		}

		private void OnDisable()
		{
			// cleanup
			if (_actionAssetList != null)
			{
				_actionAssetList.DisposeGUI();
			}
			
			if (_bucketConsiderations != null)
			{
				_bucketConsiderations.DisposeGUI();
			}
		}

		private static NestedAssetList<UtilityAIAction> CreateActionList(SerializedProperty prop)
		{
			var view = new NestedAssetList<UtilityAIAction>(prop);

			view.DefaultTypeIconGUID = "b403041b6ec9a3744b4e92bc8014f7f6";
			view.DrawTypeIcon = true;

			view.onDrawListItem = (ref Rect rect, SerializedProperty prop, UtilityAIAction so) =>
			{
				var wrect = rect.SliceRight(50);
				var newWeight = Mathf.Max(EditorGUI.FloatField(wrect, so._weight), 0f);
				if (newWeight != so._weight)
				{
					Undo.RecordObject(so, "Change weight");
					so._weight = newWeight;
				}
			};
			return view;
		}
		
		private static NestedAssetList<UtilityAIConsideration> CreateConsiderationList(SerializedProperty prop)
		{
			NestedAssetList<UtilityAIConsideration> view = new (prop);
			view.DefaultTypeIconGUID = "d8ec218438d247b49a3a0f61ed39664d";
			view.DrawTypeIcon = true;
			view.onDrawListItem = (ref Rect rect, SerializedProperty itemProp, UtilityAIConsideration so) =>
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