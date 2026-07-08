// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	[System.Serializable]
	public abstract class UAIConstraint_Object
	{
		public abstract bool Validate(object value);
	}
}