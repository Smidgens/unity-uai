// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	public abstract class UtilityAIConsideration : UtilityAISO, IUtilityConsideration
	{
		public abstract float GetScore(in UtilityAIContext context);

		protected float EvalScore(float score)
		{
			score = Mathf.Clamp01(_curve.Evaluate(score));
			return _invert ? 1 - score : score;
		}

		[SOArrayColumn(60f, true)]
		[HideInInspector]
		[SerializeField] internal bool _invert = false;
		
		[SOArrayColumn(50)]
		[HideInInspector]
		[SerializeField] internal AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
	}
}