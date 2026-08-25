// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	public enum EUAIActionStatus
	{
		Inactive,
		Active,
		Finished,
		Cancelled
	}
}

namespace Smidgenomics.Unity.UAI
{
	public enum EUAIRandomMethod
	{
		Uniform,
		Weighted
	}
}

// namespace Smidgenomics.Unity.UAI
// {
// 	/// <summary>
// 	/// Editor helper
// 	/// </summary>
// 	[System.Flags]
// 	internal enum EUAIActionOverrideFlags
// 	{
// 		None = 0,
// 		ActivateAction = 1,
// 		DeactivateAction = 2,
// 		CanCancelAction = 4,
// 		GetActionCooldown = 8,
// 		GetWeightModifier = 16,
// 	}
// }