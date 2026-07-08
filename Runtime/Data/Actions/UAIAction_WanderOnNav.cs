// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.Collections;
	using System.ComponentModel;
	using UnityEngine.AI;

	[DisplayName("Wander (Navigation)")]
	internal sealed class UAIAction_WanderOnNav : UAIAction
	{
		public override IEnumerator ActivateAction()
		{
			_interruptThreshold = Time.time + _interruptibleAfter;
			var go = GetContext().agent.gameObject;
			var navAgent = go.GetComponent<NavMeshAgent>();
			var loc = GetRandomDestination(navAgent);

			if (!navAgent.SetDestination(loc))
			{
				yield return null;
			}
			else
			{
				var dest = navAgent.destination;
				yield return new WaitUntil(() => IsAtDestination(navAgent, dest, _stopThreshold));
			}
		}

		public override IEnumerator DeactivateAction()
		{
			var go = GetContext().agent.gameObject;
			var navAgent = go.GetComponent<NavMeshAgent>();
			navAgent.SetDestination(navAgent.transform.position);

			yield return new WaitForSeconds(3);
			yield return null;
		}

		public override float GetActionCooldown()
		{
			return GetActionStatus() == EUAIActionStatus.Cancelled ? 10f : _cooldown;
		}

		public override bool CanCancelAction()
		{
			return Time.time >= _interruptThreshold;
		}

		[SerializeField, Min(0.01f)] internal float _cooldown = 1f;
		[SerializeField, Min(0.01f)] internal float _interruptibleAfter = 1f;
		
		[Header("Navigation")]
		[SerializeField, Min(0.5f)] private float _wanderRadius = 10;
		[SerializeField, Min(0.5f)] private float _maxSampleDistance = 2f;

		[Min(0.05f)]
		[SerializeField] private float _stopThreshold = 0.3f;

		private float _interruptThreshold;

		private bool IsAtDestination(NavMeshAgent agent, Vector3 location, float threshold)
		{
			if (!Mathf.Approximately(Vector3.Distance(agent.destination, location), 0f))
			{
				return true;
			}

			return Vector3.Distance(agent.transform.position, location) <= threshold;
		}

		private Vector3 GetRandomDestination(NavMeshAgent agent)
		{
			var roffset = Random.insideUnitSphere * _wanderRadius;

			var sample = agent.transform.position + roffset;

			NavMeshQueryFilter filter = new()
			{
				agentTypeID = agent.agentTypeID
			};
			
			if (NavMesh.SamplePosition(sample, out NavMeshHit hit, _maxSampleDistance, filter))
			{
				return hit.position;
			}

			return sample;

		}

	}
}