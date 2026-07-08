// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System.ComponentModel;
	using UnityEngine;

	[DisplayName("Range")]
	internal sealed class UAIConstraint_Int_Range : UAIConstraint_Int
	{
		public override int Clamp(int value)
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
		[SerializeField] private int _min;
		[EditConditionToggle(nameof(_useMax))]
		[SerializeField] private int _max = 10;

		[HideInInspector]
		[SerializeField] private bool _useMin, _useMax;
	}
}