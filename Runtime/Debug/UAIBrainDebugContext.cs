// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;

	public sealed class UAIBrainDebugContext
	{
		// last evaluated scores of all considerations in brain
		public float[] considerationScores = Array.Empty<float>();
	}
}