// smidgens @ github

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;
	using UnityEngine;

	internal readonly struct UAIAtlasIcon
	{
		// for returning default refs
		public static UAIAtlasIcon Empty = new ();
		public readonly Rect coords; //
		public readonly Texture2D atlas;

		public UAIAtlasIcon(Rect c, Texture2D a)
		{
			coords = c;
			atlas = a;
		}

		public void Draw(in Rect area, in Color c)
		{
			var tc = GUI.color;
			GUI.color = c;
			Draw(area);
			GUI.color = tc;
		}

		public void Draw(in Rect area)
		{
			if (!atlas)
			{
				EditorGUI.DrawRect(area, Color.magenta * 0.5f);
				return;
			}

			using (new GUI.ClipScope(area))
			{
				var sx = 1f / coords.size.x;
				var sy = 1f / coords.size.y;
				var ir = area;
				ir.size = new Vector2
				(
					sx * area.width,
					sy * area.width
				);
				ir.position = new Vector2
				(
					-coords.position.x * area.width * sx,
					-coords.position.y * area.height * sy
				);
				GUI.DrawTexture(ir, atlas, ScaleMode.StretchToFill);
			}
		}
	}
}

#endif