// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using System;

	[AttributeUsage(AttributeTargets.Field)]
	public sealed class SOArrayColumn : Attribute
	{
		public float width { get; }
		public bool label { get; }

		public SOArrayColumn(float width = 0, bool label = false)
		{
			this.width = width;
			this.label = label;
		}
	}
}

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.Linq;

	[System.Serializable]
	internal struct SORef<T> where T : UAIScriptableObject
	{
		public T item;
		// saved id in case ref gets lost
		public string id;
	}

	[System.Serializable]
	internal sealed class SOArray<T> where T : UAIScriptableObject
	{
		public T[] GetItems()
		{
			return _array.Select(x => x.item).Where(x => x != null).ToArray();
		}

		public ref SORef<T>[] GetArr()
		{
			return ref _array;
		}

		[SerializeField] internal SORef<T>[] _array = { };

		[HideInInspector]
		[SerializeField] internal int _selectedIndex = -1;
	}
}