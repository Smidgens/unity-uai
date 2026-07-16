// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System.ComponentModel;
	using UnityEngine;

	[DisplayName("Memory/Has Memory Value")]
	internal sealed class UAIConsideration_HasMemoryValue : UAIConsideration
	{
		public override float GetScore(in UAIAgentContext context)
		{
			return EvalScore(context.memory.HasValue(_key) ? 1 : 0);
		}

		[SerializeField] private UAIMemoryKey _key;

	}
}