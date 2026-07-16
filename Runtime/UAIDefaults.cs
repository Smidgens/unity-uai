// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	/// <summary>
	/// Default values re-used across module
	/// </summary>
	public static class UAIDefaults
	{
		public static readonly UAISelector DefaultActionSelector = new UAISelector_TopScore();
		public static readonly UAISelector DefaultBucketSelector = new UAISelector_TopScore();
		
		// default score for actions without considerations
		public const float DEFAULT_ACTION_SCORE = 1f;
	}
}