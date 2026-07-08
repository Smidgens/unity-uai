// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.ComponentModel;

	[DisplayName("Top Score")]
	public sealed class UAISelector_TopScore : UAISelector
	{
		public override int SelectIndex(int count, Func<int, float> scoreFn)
		{
			return GetBestIndex(count, scoreFn);
		}

		public override string GetDebugIconPath()
		{
			return UAIConstants.RES_PATH + "/{top}";
		}
	}
}