// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.ComponentModel;

	/// <summary>
	/// Returns score that bounces between 0 and 1 over time
	/// </summary>
	[DisplayName("Ping Pong")]
	internal sealed class UAIConsideration_PingPong : UAIConsideration
	{
		public override float GetScore(in UAIAgentContext Context)
		{
			return EvalScore((Mathf.Sin(Time.time * _speed) + 1f) / 2f);
		}

		[SerializeField] private float _speed = 100f;
	}
}