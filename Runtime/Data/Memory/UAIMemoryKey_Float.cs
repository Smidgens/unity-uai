// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System.Globalization;
	using UnityEngine;

	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Memory/Float")]
	public sealed class UAIMemoryKey_Float : UAIMemoryKey<float>
	{
		public override bool Validate(ref UAIMemoryValue value)
		{
			if (_constraint != null)
			{
				value.floatValue = _constraint.Clamp(value.floatValue);
			}
			return true;
		}

		public override string StringifyValue(in UAIMemoryValue value)
		{
			return value.floatValue.ToString(CultureInfo.InvariantCulture);
		}
		
		[SerializeReference, InstancedReference]
		private UAIConstraint_Float _constraint;
	
	}
}