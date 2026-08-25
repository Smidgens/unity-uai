// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	public abstract class UAIConsideration : UAIScriptableObject, IUAIConsideration
	{
		public abstract float GetScore(in UAIAgentContext context);

		float IUAIConsideration.GetScoreInternal(in UAIAgentContext context)
		{
			return EvalScore(GetScore(context));
		}

		/// <summary>
		/// Applies inversion flag and curve to score
		/// </summary>
		protected float EvalScore(float score)
		{
			score = Mathf.Clamp01(_curve.Evaluate(score));
			return _invert ? 1 - score : score;
		}

		// [SOArrayColumn(60f, true)]
		[HideInInspector]
		[SerializeField] internal bool _invert = false;
		
		// [SOArrayColumn(50)]
		[HideInInspector]
		[SerializeField] internal AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
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
	using System.Reflection;
	using UObject = UnityEngine.Object;
	using SP = UnityEditor.SerializedProperty;
	using RL = UnityEditorInternal.ReorderableList;

	[CustomEditor(typeof(UAIConsideration), true)]
	internal sealed class _UAIConsideration : _UAIScriptableObject
	{
		protected override bool ShouldShowOverrides()
		{
			return false;
		}
	}
}

#endif