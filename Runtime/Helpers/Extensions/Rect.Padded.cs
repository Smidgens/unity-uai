// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	internal static partial class Rect_
	{
		public static Rect Padded(this Rect r, RectOffset ro)
		{
			var center = r.center;
			r.height -= ro.bottom + ro.top;
			r.width -= ro.left + ro.right;
			r.center = center;
			return r;
		}
	}
}