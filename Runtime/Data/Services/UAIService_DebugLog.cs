// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using System.ComponentModel;
	using UnityEngine;

	[DisplayName("Debug/Debug Log")]
	internal sealed class UAIService_DebugLog : UAIService
	{
		public override void StartService()
		{
			if (_startMessage.Length > 0)
			{
				Debug.Log(_startMessage);
			}
		}

		public override void StopService()
		{
			if (_stopMessage.Length > 0)
			{
				Debug.Log(_stopMessage);
			}
		}

		[SerializeField] private string _startMessage = string.Empty;
		[SerializeField] private string _stopMessage = string.Empty;

		
	}
}