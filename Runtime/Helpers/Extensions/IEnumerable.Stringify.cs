// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System.Collections.Generic;
	using System.Linq;
	
	internal static class IEnumerable_
	{
		public static string Stringify<T>(this IEnumerable<T> arr)
		{
			int len = arr.Count();
			var s = "[";
			int i = -1;
			foreach (var value in arr)
			{
				i++;
				s += value.ToString();
				if (i < len - 1)
				{
					s += ",";
				}
			}
			s += "]";
			return s;
		}
	}
}