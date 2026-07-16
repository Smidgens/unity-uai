// smidgens @ github

#pragma warning disable CS0414

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	/// <summary>
	/// Background logic
	/// design TBD
	///
	/// Notes:
	/// - Services instances are always reused
	/// </summary>
	public abstract class UAIService : UAINode
	{
		/// <summary>
		/// Runs exactly once after service instance is created
		/// </summary>
		public virtual void InitService()
		{
			// override me
		}

		/// <summary>
		/// Runs every time service becomes active
		/// </summary>
		public virtual void StartService()
		{
			// override me
		}

		/// <summary>
		/// Runs every time service becomes inactive
		/// </summary>
		public virtual void StopService()
		{
			// override me
		}
	}
}