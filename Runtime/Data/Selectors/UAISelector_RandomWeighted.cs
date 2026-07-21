// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.ComponentModel;

	[DisplayName("Random Weighted")]
	public sealed class UAISelector_RandomWeighted : UAISelector
	{
		public override int SelectIndex(int count, Func<int, float> scoreFn)
		{
			var topIndex = GetBestIndex(count, scoreFn);
			if (topIndex < 0)
			{
				return -1;
			}
			var topScore = scoreFn.Invoke(topIndex);
			var minScore = topScore;
			CollectMinScores(count, topIndex, minScore, scoreFn, out var indices, out var scores);
			return indices[UAIMath.GetRandomArrayIndexWeighted<float>(scores)];
		}

		public override string GetDebugIconGUID()
		{
			return "a1446d554144a4944b389210a34ff6b9";
		}

		public override Rect GetDebugIconCoords()
		{
			return new Rect(0.125f * 0, 0.125f * 7, 0.125f, 0.125f);
		}
	}
}