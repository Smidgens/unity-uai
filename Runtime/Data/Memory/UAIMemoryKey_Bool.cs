// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Memory/Bool")]
	public sealed class UAIMemoryKey_Bool : UAIMemoryKey<bool>
	{
		public override string StringifyValue(in UAIMemoryValue value)
		{
			return value.boolValue ? "t" : "f";
		}
	}
}