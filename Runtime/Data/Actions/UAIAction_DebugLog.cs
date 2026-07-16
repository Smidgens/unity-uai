// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.Collections;
	using System.ComponentModel;

	[DisplayName("Debug/Debug Log")]
	internal sealed class UAIAction_DebugLog : UAIAction
	{
		public override IEnumerator ActivateAction()
		{
			yield return new WaitForSeconds(_duration);
			yield return null;
		}

		public override float GetActionCooldown()
		{
			return _cooldown;
		}

		// [Header("Debug Log")]
		[SerializeField] private string _debugText = "";
		
		[Min(0.1f)]
		[SerializeField] private float _duration = 1;
		
		[Min(UAIConstants.MIN_COOLDOWN)]
		[SerializeField] internal float _cooldown = 1f;
	}
}