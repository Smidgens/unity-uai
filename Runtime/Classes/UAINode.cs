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
	public abstract class UAINode : UAIScriptableObject
	{
		protected GameObject GetContextGameObject()
		{
			return GetContext().agent.gameObject;
		}

		protected UAIMemory GetContextMemory()
		{
			return GetContext().memory;
		}

		protected UAIAgentContext GetContext()
		{
			return _brain != null ? _brain.GetContext() : default;
		}

		// owning brain
		internal UAIBrain _brain = null;
	}
}