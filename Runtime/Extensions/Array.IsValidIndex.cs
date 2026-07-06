// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	internal static partial class Array_
	{
		public static bool IsValidIndex<T>(this T[] arr, int i)
		{
			return i >= 0 && i < arr.Length;
		}
	}
}