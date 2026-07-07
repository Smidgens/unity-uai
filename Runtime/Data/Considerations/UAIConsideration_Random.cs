// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	
	[DisplayName("Random Range")]
	internal sealed class UAIConsideration_Random : UAIConsideration
	{
		public override float GetScore(in UAIAgentContext Context)
		{
			var mn = Mathf.Min(_interval.min, _interval.max);
			var mx = Mathf.Max(_interval.min, _interval.max);
			return EvalScore(Random.Range(mn, mx));
		}

		[FloatInterval(0, 1)]
		[SerializeField] private UAIFloatRange _interval = new() { min = 0, max = 1 };
	}
}