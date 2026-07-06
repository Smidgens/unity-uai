// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using System.Collections.Generic;
	using UnityEngine;

	internal static class UAIMath
	{
		public static float ScoreConsiderations
		(
			in UtilityAIContext context,
			IEnumerable<IUtilityConsideration> considerations,
			out int count
		)
		{
			// we can't use array length if items may be null
			count = 0;

			// consideration score
			float combinedScore = 1;

			foreach(var consideration in considerations)
			{
				// we really throttled someone's terrier if we allowed a null item in here
				if(consideration == null || !consideration.Enabled)
				{
					continue;
				}
				count++;
		
				float score = consideration.GetScore(context);

				// we don't need to continue, although we might want to know other consideration scores
				// for debugging purposes even if they wouldn't change the total result
				if(Mathf.Approximately(score, 0f))
				{
					return 0;
				}

				combinedScore *= score;
			}

			// see library for explanation of what's going on here
			return RenormalizeAggregateScore(count, combinedScore);
		}

		/**
		 * Dave Mark's "magic" utility formula to renormalize aggregate score
		 * (Bring small value from multiplying scores together closer to 1)
		 */
		public static float RenormalizeAggregateScore(int ConsiderationCount, float AggregateScore)
		{
			if(ConsiderationCount <= 0){ return 0; }
			float ModFactor = 1 - (1 / ConsiderationCount);
			float MakeupValue = (1 - AggregateScore) * ModFactor;
			return AggregateScore + (MakeupValue * AggregateScore);
		}

		public static int GetRandomArrayIndexWeighted<T>(int count, System.Func<int,float> weightFn)
		{
			float[] weights = new float[count];
			for (int i = 0; i < count; i++)
			{
				weights[i] = weightFn.Invoke(i);
			}
			return GetRandomArrayIndexWeighted(weights, (in float w) => w);
		}
		
		public static int GetRandomArrayIndexWeighted<T>(in IList<float> weights)
		{
			return GetRandomArrayIndexWeighted(weights, (in float w) => w);
		}

		public static int GetRandomArrayIndexWeighted<T>(in IList<T> arr, FuncRefRO<T, float> weightFn)
		{
			if (arr.Count == 0)
			{
				return -1;
			}
			List<float> weights = new();
			List<float> cumulative = new();

			float totalWeight = 0;
			for (var i = 0; i < arr.Count; i++)
			{
				float weight = weightFn.Invoke(arr[i]);
				weights.Add(weight);
				totalWeight += weight;
				cumulative.Add(totalWeight);
			}

			var max = cumulative[cumulative.Count - 1];
			var rand = max * UnityEngine.Random.Range(0f, 1f);
			return cumulative.FindIndex(x => x >= rand);
		}
	}
}