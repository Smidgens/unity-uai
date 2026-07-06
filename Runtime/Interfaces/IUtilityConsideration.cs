// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	
	/**
	 * 
	 */
	public interface IUtilityConsideration
	{
		public bool Enabled { get;  }
		
		// display info
		public string Name { get; }

		public float GetScore(in UtilityAIContext context);
	}
	
}
