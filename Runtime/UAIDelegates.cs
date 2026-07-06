// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	// Readonly ref input
	public delegate void ActionRefRO<T>(in T item);
	
	// Readonly ref input with return value
	public delegate R FuncRefRO<T, out R>(in T item);
}
