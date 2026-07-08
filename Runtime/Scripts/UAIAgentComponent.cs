// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;

	[AddComponentMenu(UAIConstants.AC_PATH + "UAI Agent")]
	[DisallowMultipleComponent]
	public sealed class UAIAgentComponent : MonoBehaviour, IUAIAgent
	{
		[SerializeField] private UAIBehaviour _template;
		[SerializeField] internal bool _debugActivity;

		private UAIBrain _brain;

		private void Start()
		{
			_brain = UAIFactory.CreateBrain(new UAIBrainInitConfig
			{
				agent = this,
				behaviourTemplate = _template
			});
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