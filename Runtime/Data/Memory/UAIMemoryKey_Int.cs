// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	// TODO: More robust inspector
	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Memory/Int")]
	public sealed class UAIMemoryKey_Int : UAIMemoryKey
	{
		public override bool Validate(ref UAIMemoryValue value)
		{
			return true;
		}
	}
}