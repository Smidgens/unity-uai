// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Memory/Int")]
	public sealed class UAIMemoryKey_Int : UAIMemoryKey<float>
	{
		public override bool Validate(ref UAIMemoryValue value)
		{
			if (_constraint != null)
			{
				value.intValue = _constraint.Clamp(value.intValue);
			}
			return true;
		}

		public override string StringifyValue(in UAIMemoryValue value)
		{
			return value.intValue.ToString();
		}

		[SerializeReference, InstancedReference]
		private UAIConstraint_Int _constraint;
	}
}