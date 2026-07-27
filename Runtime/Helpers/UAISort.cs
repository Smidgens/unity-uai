// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using System.Collections.Generic;

	public static class UAISort
	{
		public static void IndicesByWeight(ref int[] indices, int i, int n, Func<int, float> fn, bool asc = true)
		{
			Array.Sort(indices, i, n, new CompareByPredicate(fn, asc));
		}

		private readonly struct CompareByPredicate : IComparer<int>
		{
			public CompareByPredicate(Func<int, float> fn, bool desc)
			{
				this.fn = fn;
				this.desc = desc;
			}

			public int Compare(int a, int b)
			{
				return desc
				? fn.Invoke(a).CompareTo(fn.Invoke(b))
				: fn.Invoke(b).CompareTo(fn.Invoke(a));
			}
			private readonly Func<int, float> fn;
			private readonly bool desc;
		}
	}
}