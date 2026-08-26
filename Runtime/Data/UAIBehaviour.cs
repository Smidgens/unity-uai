// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using UnityEngine.Serialization;

	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Behaviour")]
	public sealed class UAIBehaviour : UAIScriptableObject
	{
		[System.Serializable]
		public struct BucketExecutionConfig
		{
			[HideInInspector]
			public bool enabled;

			[EditConditionToggle(nameof(enabled), label:null)]
			public UAIBucket bucket;
			
			[UAIHeader("Overrides")]
		
			[UAIIndent]
			[FormerlySerializedAs("overrideConsiderations")] [EditConditionToggle(nameof(enableConsiderations))]
			public UAIConsiderationList considerations;

			[UAIIndent]
			[FormerlySerializedAs("overrideSelector")]
			[EditConditionToggle(nameof(enableSelector))]
			[SerializeReference, UAIInstancedReference]
			public UAISelector selector;
		
			[UAIIndent]
			[FormerlySerializedAs("overrideWeight")]
			[EditConditionToggle(nameof(enableWeight))]
			[Min(0f)]
			public float weight; 

			[HideInInspector] public bool enableConsiderations;
			[HideInInspector] public bool enableWeight;
			[HideInInspector] public bool enableSelector;
		}

		[SerializeReference, UAIInstancedReference(defaultValueLabel = "Default")]
		internal UAISelector _bucketSelector = new UAISelector_TopScore();
		
		[UAIExpand(true)]
		[HideInInspector]
		[SerializeField] internal BucketExecutionConfig[] _buckets = { };
	}
}


#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using System;
	using UnityEditor;
	using UnityEditorInternal;
	using UnityEngine;

	[CustomEditor(typeof(UAIBehaviour))]
	internal sealed class _UAIBehaviour : _UAIScriptableObject
	{
		protected override bool ShouldShowTypeInfo() => false;
		
		protected override bool ShouldGroupFields() => false;

		protected override void OnAfterFields()
		{
			serializedObject.UpdateIfRequiredOrScript();
			EditorGUILayout.Space();
			_bucketList.DoLayoutList();
			serializedObject.ApplyModifiedProperties();
		}

		public override bool RequiresConstantRepaint()
		{
			// hack because undo refresh is flaky for SerializeReference
			return true;
		}


		private ReorderableList _bucketList;

		protected override void OnInit()
		{
			var bProp = serializedObject.FindProperty(nameof(UAIBehaviour._buckets));
			_bucketList = new ReorderableList(serializedObject, bProp, true, true, true, true)
			{
				elementHeightCallback = i =>
				{
					var itemProp = bProp.GetArrayElementAtIndex(i);
					
					
					var enabledProp = itemProp.FindPropertyRelative("enabled");

					var h = EditorGUIUtility.standardVerticalSpacing * 2f;

					if (!enabledProp.boolValue)
					{
						return h + EditorGUIUtility.singleLineHeight;
					}
					return h + EditorGUI.GetPropertyHeight(itemProp);
				},
				drawElementCallback = (r, i, f, a) =>
				{
					r.SliceTop(EditorGUIUtility.standardVerticalSpacing);
					r.SliceBottom(EditorGUIUtility.standardVerticalSpacing);
					
					var itemProp = bProp.GetArrayElementAtIndex(i);
					var enabledProp = itemProp.FindPropertyRelative("enabled");
					var bucketProp = itemProp.FindPropertyRelative("bucket");

					if (!enabledProp.boolValue)
					{
						EditorGUI.PropertyField(r, bucketProp, GUIContent.none);
					}
					else
					{
						EditorGUI.PropertyField(r, itemProp, GUIContent.none, true);
					}
					
				},
				drawHeaderCallback = r =>
				{
					EditorGUI.LabelField(r, bProp.displayName);
				}
			};
		}
	}
}

#endif