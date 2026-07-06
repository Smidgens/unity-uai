// smidgens @ github

#pragma warning disable CS0414
// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Base class for actions and services
	/// </summary>
	public abstract class UtilityAIBehaviourNode : UtilityAISO
	{
		protected GameObject GetContextGameObject()
		{
			return GetContext().gameObject;
		}

		protected UtilityAIMemory GetContextMemory()
		{
			return GetContext().memory;
		}

		protected UtilityAIContext GetContext()
		{
			return _brain != null ? _brain.GetContext() : default;
		}

		// owning brain
		internal UtilityAIBrain _brain = null;
	}
}