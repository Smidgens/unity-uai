// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;

	/// <summary>
	/// Utility AI Exception
	/// </summary>
	public sealed class UAIException : Exception
	{
		public UAIException()
		{
		}

		public UAIException(string message) : base(message)
		{
		}

		public UAIException(string message, Exception inner) : base(message, inner)
		{
		}
	}
}