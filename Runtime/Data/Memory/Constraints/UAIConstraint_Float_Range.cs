// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System.ComponentModel;
	using UnityEngine;

	[DisplayName("Range")]
	internal sealed class UAIConstraint_Float_Range : UAIConstraint_Float
	{
		public override float Clamp(float value)
		{
			if (_useMin && _useMax)
			{
				return Mathf.Clamp(value, Mathf.Min(_min, _max), Mathf.Max(_min, _max));
			}

			if (_useMin)
			{
				return value < _min ? _min : value;
			}

			if (_useMax)
			{
				return value > _max ? _min : value;
			}
			
			return value;
		}

		[EditConditionToggle(nameof(_useMin))]
		[SerializeField] private float _min;
		[EditConditionToggle(nameof(_useMax))]
		[SerializeField] private float _max = 10f;

		[HideInInspector]
		[SerializeField] private bool _useMin, _useMax;
	}
}