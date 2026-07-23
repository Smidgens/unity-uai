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
		
		public const float DEFAULT_ACTION_SCORING_RATE = 1f;
		public const float DEFAULT_BUCKET_SCORING_RATE = 5f;
		
		// default score for actions without considerations
		public const float DEFAULT_ACTION_SCORE = 1f;
	}
}