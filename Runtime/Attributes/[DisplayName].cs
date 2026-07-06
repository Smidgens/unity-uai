// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DisplayNameAttribute : Attribute
	{
		public string displayName { get; private set; }

		public DisplayNameAttribute(string dname)
		{
			displayName = dname;
		}
	}
}