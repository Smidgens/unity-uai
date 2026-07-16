// smidgens @ github

#pragma warning disable 0414

// #if ENABLE_NAVIGATION_UI_REQUIRES_PACKAGE

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.Collections;
	using System.ComponentModel;
	using UnityEngine.AI;

	[DisplayName("Navigation/Destination from Memory")]
	internal sealed class UAIAction_NavFromMemory : UAIAction
	{
		public override IEnumerator ActivateAction()
		{
			GetContext().memory.TryGetObject(_memoryKey, out var outOb);

			if (outOb is not (Transform or GameObject))
			{
				FinishAction();
				yield break;
			}

			var tPos = SetDestination(outOb);
			yield return new WaitUntil(() => IsFinishedMoving(tPos));
		}

		public override IEnumerator DeactivateAction()
		{
			var go = GetContext().agent.gameObject;
			var navAgent = go.GetComponent<NavMeshAgent>();
			navAgent.SetDestination(navAgent.transform.position);
			yield return new WaitForSeconds(_deactivationDuration);
			yield return null;
		}

		public override float GetActionCooldown()
		{
			return GetActionStatus() == EUAIActionStatus.Cancelled ? _cancelledCooldown : _cooldown;
		}

		[SerializeField] private UAIMemoryKey_Object _memoryKey;

		[SerializeField, Min(0.01f)] internal float _cooldown = 1f;
		[SerializeField, Min(0.01f)] internal float _cancelledCooldown = 10f;
		[SerializeField, Min(0.01f)] internal float _interruptibleAfter = 1f;
		[SerializeField, Min(0f)] internal float _deactivationDuration;

		[Min(0.05f)]
		[SerializeField] private float _stopThreshold = 0.3f;

		private Vector3 SetDestination(object ob)
		{
			var t = ob is Transform
			? (Transform)ob
			: ((GameObject)ob).transform;
			var na = GetContext().agent.gameObject.GetComponent<NavMeshAgent>();
			na.SetDestination(t.position);
			return t.position;
		}

		private bool IsFinishedMoving(Vector3 originalTarget)
		{
			var go = GetContext().agent.gameObject;
			var navAgent = go.GetComponent<NavMeshAgent>();

			var dist = Vector3.Distance(go.transform.position, originalTarget);
			
			if (navAgent.isStopped || dist < _stopThreshold)
			{
				return true;
			}

			return false;
		}
		

		

	}
}