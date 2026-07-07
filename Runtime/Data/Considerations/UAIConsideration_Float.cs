// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	
	[DisplayName("Float")]
	internal sealed class UAIConsideration_Float : UAIConsideration
	{
		public override float GetScore(in UAIAgentContext Context) => _value;

		[Range(0, 1)]
		[SerializeField] private float _value = 1f;
	}
}