// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	/// <summary>
	/// Service API
	/// </summary>
	public interface IUAIService
	{
		public string Name { get; }
		public bool Enabled { get; }

		public void InitService();

		public void StartService();

		public void StopService();
		public IUAIService Clone(UAIBrain owningBrain);

	}
}