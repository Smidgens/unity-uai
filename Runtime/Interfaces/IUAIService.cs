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
		void InitService();

		void StartService();

		void StopService();

	}
}