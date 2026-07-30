// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System.ComponentModel;
	using UnityEngine;

	// [CreateAssetMenu(menuName = UAIConstants.SO_CREATE_PATH + "Service/Clear Memory")]
	[DisplayName("Memory/Clear Memory")]
	internal sealed class UAIService_ClearMemory : UAIService
	{
		public override void StartService()
		{
			if (_onStart)
			{
				ClearMemory();
			}
		}

		public override void StopService()
		{
			if (_onStop)
			{
				ClearMemory();
			}
		}

		[SerializeField] private bool _onStart;
		[SerializeField] private bool _onStop;

		[UAIHideLabel]
		[SerializeField] private UAIMemoryKey[] _specificKeys = { };

		private void ClearMemory()
		{
			var memory = GetContext().memory;
			if (memory == null)
			{
				return;
			}
			if (_specificKeys.Length > 0)
			{
				foreach (var key in _specificKeys)
				{
					memory.ClearValue(key);
				}
			}
			else
			{
				memory.ClearAllValues();
			}
			
#if SM_DEV
			Debug.Log("Memory cleared");
#endif
		}
		
	}
}