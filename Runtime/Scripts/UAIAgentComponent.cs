// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;
	using UnityEngine.Serialization;

	[AddComponentMenu(UAIConstants.AC_PATH + "UAI Agent")]
	[DisallowMultipleComponent]
	public sealed class UAIAgentComponent : MonoBehaviour, IUAIAgent
	{
		public UAIMemory agentMemory => _brain?.GetMemory();

		[FormerlySerializedAs("_template")]
		[SerializeField] private UAIBehaviour _behaviour;

		private UAIBrain _brain;

		private void Awake()
		{
			_brain = UAIFactory.CreateBrain(new UAIBrainInitConfig
			{
				agent = this,
				behaviourTemplate = _behaviour
			});

		}

		private void Start()
		{
			_brain.StartLogic();
		}

		private void OnDisable()
		{
			_brain.StopLogic();
		}

		private void OnDestroy()
		{
			_brain.Dispose();
		}

	}
}