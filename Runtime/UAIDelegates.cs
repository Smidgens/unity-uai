// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	// Readonly ref input
	public delegate void ActionRefRO<T>(in T item);
	
	public delegate void ActionRefRO<T1, T2>(in T1 v1, in T2 v2);
	
	// Readonly ref input with return value
	public delegate R FuncRefRO<T, out R>(in T item);
}
