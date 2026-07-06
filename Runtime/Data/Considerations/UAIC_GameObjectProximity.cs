// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[DisplayName("GameObject Proximity")]
	internal sealed class UAIC_GameObjectProximity : UtilityAIConsideration
	{
		public override float GetScore(in UtilityAIContext context)
		{
			if (!context.memory.TryGetObjectAsType<GameObject>(_key, out var go))
			{
				return 0f;
			}
			var selfPos = context.gameObject.transform.position;
			var dist = Vector3.Distance(selfPos, go.transform.position);
			if (dist > _maxDistance)
			{
				return 0;
			}
			return EvalScore(_scaleByDistance ? 1 - Mathf.Clamp01(dist / _maxDistance) : 1);
		}

		[SerializeField] private UAIMK_Object _key = null;
		[SerializeField] private float _maxDistance = 5;
		[SerializeField] private bool _scaleByDistance = true;

	}
}