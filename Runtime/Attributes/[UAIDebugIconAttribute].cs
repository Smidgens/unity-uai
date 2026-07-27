// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using System.Diagnostics;
	using UnityEngine;

	/// <summary>
	/// Used by UAI editor
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	[Conditional("UNITY_EDITOR")]
	internal sealed class UAIDebugIconAttribute : System.Attribute
	{
		public UAIDebugIconAttribute(string textureGUID)
		{
			guid = textureGUID;
			position = new Rect(0f, 0f, 1f, 1f);
		}

		public UAIDebugIconAttribute(string textureGUID, float x, float y, float w, float h)
		{
			guid = textureGUID;
			position = new Rect(x, y, w, h);
		}

		internal readonly string guid;
		internal readonly Rect position;
	}
}