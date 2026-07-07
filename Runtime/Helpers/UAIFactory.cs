// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	public static class UAIFactory
	{
		public static UAIBrain CreateBrain(in UAIBrainInitConfig config)
		{
			return UAIBrain.CreateBrain(config);
		}
	}
}