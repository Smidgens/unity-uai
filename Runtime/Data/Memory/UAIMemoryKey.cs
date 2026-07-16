// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	public abstract class UAIMemoryKey : ScriptableObject
	{
		public string Label => _label;
		
		public abstract Type MemoryType { get; }

		public virtual bool Validate(ref UAIMemoryValue value)
		{
			return true;
		}

		public virtual string StringifyValue(in UAIMemoryValue value)
		{
			return "";
		}

		[SerializeField] internal string _label = "Key";
		
		[Multiline]
		[SerializeField] internal string _comment = "";
	}
	
	public abstract class UAIMemoryKey<T> : UAIMemoryKey
	{
		public sealed override Type MemoryType => typeof(T);
	}
}

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using System;
	using UnityEditor;

	[CustomEditor(typeof(UAIMemoryKey), true)]
	internal sealed class _UAIMemoryKey : Editor
	{
		protected override bool ShouldHideOpenButton() => true;
	}
}


#endif