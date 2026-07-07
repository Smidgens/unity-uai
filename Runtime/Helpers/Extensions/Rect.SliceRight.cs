// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	internal static partial class Rect_
	{
		/// <summary>
		/// Slices a rect of width w from right and returns it.
		/// Note: changes original rect
		/// </summary>
		/// <param name="r">Rect</param>
		/// <param name="w">Slice width</param>
		/// <returns>Right slice</returns>
		public static Rect SliceRight(this ref Rect r, in float w)
		{
			var r2 = r;
			r2.width = w;
			r.width -= w;
			r2.x += r.width;
			return r2;
		}
	}
}