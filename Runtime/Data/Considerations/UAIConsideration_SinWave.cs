// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[DisplayName("Sin Wave")]
	internal sealed class UAIConsideration_SinWave : UAIConsideration
	{
		public override float GetScore(in UAIAgentContext Context)
		{
			return EvalScore((Mathf.Sin(Time.time * _speed) + 1f) / 2f);
		}

		[SerializeField] private float _speed = 100f;
	}
}