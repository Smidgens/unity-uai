// smidgens @ github

#pragma warning disable CS0414

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	public abstract class UtilityAIService : UtilityAIBehaviourNode
	{
		public virtual void StartService()
		{
			// override me
		}

		public virtual void StopService()
		{
			// override me
		}
	}
}