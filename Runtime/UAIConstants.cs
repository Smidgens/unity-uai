// smidgens @ github

// ReSharper disable All

#pragma warning disable 0414
#pragma warning disable 0067

namespace Smidgenomics.Unity.UAI
{
	using Color = UnityEngine.Color;

	internal static class UAIConstants
	{
		public const string SO_CREATE_PATH = "Utility AI/";
		public const string ICON_RES_PATH = "smidgenomics.uai";
		public const float MIN_COOLDOWN = 0.01f;
		public const float DEFAULT_ACTION_SR = 1;
		public const float DEFAULT_BUCKET_SR = 5;
		public static readonly Color COLOR_ACTION_ACTIVE = Color.green;
		public static readonly Color COLOR_SELECTABLE = Color.white;
		public static readonly Color COLOR_MUTED = new Color(0.7f, 0.7f, 0.7f);
		public static readonly Color COLOR_ACTION_DEACTIVATING = Color.yellow;
		public static readonly Color COLOR_ACTION_CANCELLED = Color.blue;
		public static readonly Color COLOR_ACTION_FINISHED = Color.cyan;
		public static readonly Color COLOR_ACTION_UNCANCELLABLE = Color.magenta;
		public static readonly Color COLOR_ACTION_SUSTAINED = new Color(0.5f, 0f, 0.5f);
	}
}