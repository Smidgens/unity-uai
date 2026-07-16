// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	/// <summary>
	/// 
	/// </summary>
	public interface IUAIConsideration
	{
		public bool Enabled { get;  }
		
		// display info
		public string Name { get; }

		public float GetScore(in UAIAgentContext context);
	}
	
}
