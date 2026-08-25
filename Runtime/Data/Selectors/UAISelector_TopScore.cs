// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.ComponentModel;

	[DisplayName("Top Score")]
	[System.Serializable]
	public sealed class UAISelector_TopScore : UAISelector
	{
		public override int SelectIndex(int count, Func<int, float> scoreFn)
		{
			return GetBestIndexFromSortedDesc(count, scoreFn);
		}

		public override (string, Rect) GetDebugIcon()
		{
			return ("a1446d554144a4944b389210a34ff6b9", new Rect(0.125f, 0.125f * 7, 0.125f, 0.125f));
		}
	}
}