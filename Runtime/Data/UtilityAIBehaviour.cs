// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Behaviour")]
	public sealed class UtilityAIBehaviour : ScriptableObject
	{
		[System.Serializable]
		public struct BucketExecutionConfig
		{
			public UtilityAIBucket bucket;
		
			[EditConditionToggle(nameof(overrideConsiderations))]
			public UtilityConsiderationSetSO considerations;

			[EditConditionToggle(nameof(overrideSelector))]
			[InstancedReference]
			[SerializeReference]
			public UtilityAISelector actionSelector;
		
			[EditConditionToggle(nameof(overrideWeight))]
			[Min(0f)]
			public float weight; 

			[HideInInspector] public bool overrideConsiderations;
			[HideInInspector] public bool overrideWeight;
			[HideInInspector] public bool overrideSelector;
		}
		
		[InstancedReference]
		[SerializeReference]
		internal UtilityAISelector _bucketSelector = new UtilityAISelector_TopScore();
		
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

	[CustomEditor(typeof(UtilityAIBehaviour), true)]
	internal sealed class _UtilityAIBehaviour : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
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