// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	public abstract class UAIMemoryKey : ScriptableObject
	{
		public virtual bool Validate(ref UAIMemoryValue value)
		{
			return true;
		}
		
		[SerializeField] internal string _label = "Key";
		
		[Multiline]
		[SerializeField] internal string _comment = "";
	}
}