// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	/// <summary>
	/// Delegate signatures used across module
	/// </summary>
	public static class UAIDelegates
	{
		/// <summary>
		/// Readonly ref args (1)
		/// </summary>
		public delegate void ActionRefRO<T>(in T item);
		
		/// <summary>
		/// Readonly ref args (2)
		/// </summary>
		public delegate void ActionRefRO<T1, T2>(in T1 v1, in T2 v2);

		/// <summary>
		/// Readonly ref args (1)
		/// </summary>
		public delegate R FuncRefRO<T, out R>(in T item);
	}
}
