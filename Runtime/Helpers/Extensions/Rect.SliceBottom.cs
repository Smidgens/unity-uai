// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	internal static partial class Rect_
	{
		public static Rect SliceBottom(this ref Rect r, in float h)
		{
			var r2 = r;
			r2.height = h;
			r.height -= h;
			r2.y += r.height;
			return r2;
		}
	}
}
