// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	[UAIListField(nameof(_curve), 25f)]
	[UAIListField(nameof(_invert), 22f)]
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

		[HideInInspector]
		[Tooltip("Invert score")]
		[UAIToggleIcon(24,25)]
		[SerializeField] internal bool _invert;
		
		[HideInInspector]
		[UAICurve(x:0,y:0,w:1, h:1)]
		[SerializeField] internal AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;

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