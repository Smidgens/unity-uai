// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	// TODO: More robust inspector
	[CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Memory/Float")]
	public sealed class UAIMemoryKey_Float : UAIMemoryKey
	{
		public override bool Validate(ref UAIMemoryValue value)
		{
			return true;
		}
	
	}
}