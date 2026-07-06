// smidgens @ github

// ReSharper disable All

using System.Reflection;

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;

	[System.Serializable]
	public abstract class UtilityAISelector
	{
		// Assumes sorted values in descending order
		public abstract int SelectIndex(int count, Func<int, float> scoreFn);

		public virtual string GetDebugIconPath()
		{
			return UAIConstants.ICON_RES_PATH + "/{random}";
		}

		public string GetDisplayName()
		{
			var dn = GetType().GetCustomAttribute<DisplayNameAttribute>();
			return dn != null ? dn.displayName : GetType().Name;
		}
		
		protected int GetBestIndex(int count, Func<int, float> scoreFn)
		{
			for (var i = 0; i < count; i++)
			{
				if (Mathf.Approximately(scoreFn.Invoke(i), 0))
				{
					continue;
				}
				return i;
			}
			return -1;
		}

		protected void CollectMinScores
		(
			int count, int startIndex,
			float minScore,
			Func<int, float> scoreFn,
			out List<int> indices,
			out List<float> scores
		)
		{
			indices = new List<int>();
			scores = new List<float>();
			for (var i = startIndex; i < count; i++)
			{
				var score = scoreFn.Invoke(i);
				if (score < minScore || Mathf.Approximately(score, 0))
				{
					continue;
				}
				indices.Add(i);
				scores.Add(score);
			}
		}
	}
}

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[DisplayName("Top Score")]
	public sealed class UtilityAISelector_TopScore : UtilityAISelector
	{
		public override int SelectIndex(int count, Func<int, float> scoreFn)
		{
			return GetBestIndex(count, scoreFn);
		}
		
		public override string GetDebugIconPath()
		{
			return UAIConstants.ICON_RES_PATH + "/{top}";
		}
	}
}

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[DisplayName("Top Score Percentage")]
	public sealed class UtilityAISelector_TopScoreInterval : UtilityAISelector
	{
		public override int SelectIndex(int count, Func<int, float> scoreFn)
		{
			var topIndex = GetBestIndex(count, scoreFn);

			if (topIndex < 0)
			{
				return -1;
			}
			var topScore = scoreFn.Invoke(topIndex);
			var minScore = topScore - (topScore * _interval);
			CollectMinScores(count, topIndex, minScore, scoreFn, out var indices, out var scores);
			if (indices.Count == 1)
			{
				return topIndex;
			}
			if (_randomMethod == EUtilityAIRandomMethod.Weighted)
			{
				return indices[UAIMath.GetRandomArrayIndexWeighted<float>(scores)];
			}
			return indices[UnityEngine.Random.Range(0, indices.Count - 1)];
		}
		
		public override string GetDebugIconPath()
		{
			return UAIConstants.ICON_RES_PATH + "/{top_interval}";
		}

		[Range(0, 1)]
		[SerializeField] private float _interval = 0.25f;

		[SerializeField] private EUtilityAIRandomMethod _randomMethod = EUtilityAIRandomMethod.Uniform;
	}
}

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	[DisplayName("Random Weighted")]
	public sealed class UtilityAISelector_RandomWeighted : UtilityAISelector
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
		
		public override string GetDebugIconPath()
		{
			return UAIConstants.ICON_RES_PATH + "/{random}";
		}
	}
}