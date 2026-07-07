// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Behaviour")]
	public sealed class UAIBehaviour : ScriptableObject
	{
		[System.Serializable]
		public struct BucketExecutionConfig
		{
			public UAIBucket bucket;
		
			[EditConditionToggle(nameof(enableConsiderations))]
			public UAIConsiderationList overrideConsiderations;

			[EditConditionToggle(nameof(enableSelector))]
			[SerializeReference, InstancedReference]
			public UAISelector overrideSelector;
		
			[EditConditionToggle(nameof(enableWeight))]
			[Min(0f)]
			public float overrideWeight; 

			[HideInInspector] public bool enableConsiderations;
			[HideInInspector] public bool enableWeight;
			[HideInInspector] public bool enableSelector;
		}

		[SerializeReference, InstancedReference(defaultValueLabel = "Default")]
		internal UAISelector _bucketSelector = new UAISelector_TopScore();
		
		[SerializeField] internal BucketExecutionConfig[] _buckets = { };
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

	[CustomEditor(typeof(UAIBehaviour), true)]
	internal sealed class _UAIBehaviour : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UAIBehaviour._bucketSelector)));
			EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UAIBehaviour._buckets)));
			serializedObject.ApplyModifiedProperties();
		}

		public override bool RequiresConstantRepaint()
		{
			// hack because undo refresh is flaky for SerializeReference
			return true;
		}

		protected override bool ShouldHideOpenButton()
		{
			return true;
		}

		private void OnEnable()
		{
		
		}

		private void OnDisable()
		{
			
		}

	}
}

#endif