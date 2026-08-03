// smidgens @ github

#pragma warning disable 0414

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.Collections;
	using System.ComponentModel;
	using UnityEngine.AI;

	[DisplayName("Navigation/Wander")]
	internal sealed class UAIAction_WanderOnNav : UAIAction
	{
		public override IEnumerator ActivateAction()
		{
			_interruptThreshold = Time.time + _interruptibleAfter;
			var go = GetContext().agent.gameObject;
			var navAgent = go.GetComponent<NavMeshAgent>();
			var loc = GetRandomDestination(navAgent);
			
			if (!navAgent || !navAgent.gameObject.activeInHierarchy)
			{
				yield break;
			}

			if (!navAgent.enabled)
			{
				navAgent.enabled = true;
			}

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

			if (!navAgent || !navAgent.gameObject.activeInHierarchy || !navAgent.enabled)
			{
				yield break;
			}

			navAgent.SetDestination(navAgent.transform.position);

			yield return new WaitForSeconds(_deactivationDuration);
			yield return null;
		}

		public override float GetActionCooldown()
		{
			return GetActionStatus() == EUAIActionStatus.Cancelled ? _cancelledCooldown : _cooldown;
		}

		public override bool CanCancelAction()
		{
			return Time.time >= _interruptThreshold;
		}

		[SerializeField, Min(0f)] internal float _cooldown = 1f;
		[SerializeField, Min(0f)] internal float _cancelledCooldown = 10f;
		[SerializeField, Min(0.01f)] internal float _interruptibleAfter = 1f;
		[SerializeField, Min(0f)] internal float _deactivationDuration;
		
		[Header("Navigation")]
		[SerializeField, Min(0.5f)] private float _wanderRadius = 10;
		[SerializeField, Min(0.5f)] private float _maxSampleDistance = 2f;
		[SerializeField, Range(5f, 45f)] private float _wanderTurnAngle = 15f;
		[SerializeField, Range(1, 10)] private float _sampleRetries = 6;

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
			var randomDir = agent.transform.forward;

			var turnAngle = _wanderTurnAngle;
			var retryStep = (360f - turnAngle) / _sampleRetries;

			NavMeshQueryFilter filter = new()
			{
				agentTypeID = agent.agentTypeID,
				areaMask = agent.areaMask
			};

			var randSign = Random.value > 0.5 ? 1 : -1;
			var randAngle = Random.Range(0f, turnAngle) * randSign;
			var wanderDist = Random.Range(_wanderRadius * 0.5f, _wanderRadius);

			for (int i = 0; i < _sampleRetries; i++)
			{
				var angle = randAngle + (i * retryStep * randSign);
				var q = Quaternion.Euler(Vector3.up * angle);
				var wDir = q * randomDir;
				var tPoint = agent.transform.position + wDir * wanderDist;

				if (NavMesh.SamplePosition(tPoint, out NavMeshHit hit, _maxSampleDistance, filter))
				{
					return hit.position;
				}
			}

			return agent.transform.position;

		}

	}
}