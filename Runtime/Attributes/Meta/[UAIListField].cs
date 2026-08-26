// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using System.Diagnostics;
	using UnityEngine;

	/// <summary>
	/// Declares field in asset to be inlineable in asset lists
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	[Conditional("UNITY_EDITOR")]
	internal sealed class UAIListFieldAttribute : Attribute
	{
		public UAIListFieldAttribute(string field, float width)
		{
			this.field = field;
			this.width = Mathf.Max(0f, width);
		}
		
		internal string field { get; }
		internal float width { get; }
	}
}