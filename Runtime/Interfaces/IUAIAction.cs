// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using IEnumerator = System.Collections.IEnumerator;

	/// <summary>
	/// Action API
	/// </summary>
	public interface IUAIAction
	{
		// display info
		public string Name { get; }

		// selectable?
		public bool Enabled { get; }

		// currently cancelable?
		public bool CanCancelAction();

		// cooldown based on current state
		public float GetActionCooldown();

		// execution status
		public EUAIActionStatus GetActionStatus();

		// main logic execution routine
		public IEnumerator ActivateAction();

		// begin cancellation
		public IEnumerator DeactivateAction();

	}
}