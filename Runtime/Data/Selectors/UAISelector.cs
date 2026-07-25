// smidgens @ github

// ReSharper disable All



namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.ComponentModel;

	[System.Serializable]
	public abstract class UAISelector
	{
		// Assumes sorted values in descending order
		public abstract int SelectIndex(int count, Func<int, float> scoreFn);

		public virtual (string, Rect) GetDebugIcon()
		{
			return ("a1446d554144a4944b389210a34ff6b9", new Rect(0, 0.125f * 7, 0.125f, 0.125f));
		}

		public string GetDisplayName()
		{
			var dn = GetType().GetCustomAttribute<DisplayNameAttribute>();
			return dn != null ? dn.DisplayName : GetType().Name;
		}
		
		// assumes descending order
		protected int GetBestIndex(int count, Func<int, float> scoreFn)
		{
			for (var i = 0; i < count; i++)
			{
				var s = scoreFn.Invoke(i);
				if (Mathf.Approximately(s, 0f))
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