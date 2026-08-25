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
		public UAIBrain agentBrain => _brain;

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

		private void OnEnable()
		{
			// if (!_brain.IsRunning())
			// {
			// 	_brain?.StartLogic();
			// }
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

#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;

	[CustomEditor(typeof(UAIAgentComponent))]
	internal sealed class _UAIAgentComponent : _UAIInspector
	{
		protected override bool ShouldShowTypeInfo() => false;

		protected override bool ShouldGroupFields() => false;
	}

}

#endif