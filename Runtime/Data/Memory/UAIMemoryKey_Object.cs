// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	// TODO: More robust inspector for type info
	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Memory/Object")]
	public sealed class UAIMemoryKey_Object : UAIMemoryKey<object>
	{
		public override bool Validate(ref UAIMemoryValue value)
		{
			if (_constraint == null)
			{
				return true;
			}
			return _constraint.Validate(value.objectRef);
		}

		public override string StringifyValue(in UAIMemoryValue value)
		{
			return value.objectRef == null ? "<null>" : value.objectRef.ToString();
		}

		[SerializeReference, InstancedReference]
		private UAIConstraint_Object _constraint = new UAIConstraint_Object_BaseType();

	}
}