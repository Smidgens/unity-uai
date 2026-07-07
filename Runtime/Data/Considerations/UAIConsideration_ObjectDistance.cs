// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[DisplayName("Object Distance")]
	internal sealed class UAIConsideration_ObjectDistance : UAIConsideration
	{
		public override float GetScore(in UAIAgentContext context)
		{
			if (!context.memory.TryGetObjectAsType<GameObject>(_key, out var go))
			{
				return 0f;
			}
			var selfPos = context.agent.gameObject.transform.position;
			var dist = Vector3.Distance(selfPos, go.transform.position);
			if (dist > _maxDistance)
			{
				return 0;
			}
			return EvalScore(_scaleByDistance ? 1 - Mathf.Clamp01(dist / _maxDistance) : 1);
		}

		[SerializeField] private UAIMemoryKey_Object _key = null;
		[SerializeField] private float _maxDistance = 5;
		[SerializeField] private bool _scaleByDistance = true;

	}
}