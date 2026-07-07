// smidgens @ github

// ReSharper disable All

using System.Reflection;

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;

	[System.Serializable]
	public abstract class UAISelector
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
				var s = scoreFn.Invoke(i);
				if (Mathf.Approximately(s, 0))
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