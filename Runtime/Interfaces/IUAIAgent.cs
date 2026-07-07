// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	/// <summary>
	/// Agent on which utility logic runs
	/// </summary>
	public interface IUAIAgent
	{
		public GameObject gameObject { get; }
	}
}