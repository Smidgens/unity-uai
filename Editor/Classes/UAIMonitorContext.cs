// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;

	internal sealed class UAIMonitorContext
	{
		public void InitGUI()
		{
			_styles = UAIEditorStyles.CreateInstance();
		}

		private UAIEditorStyles _styles;
		private Texture2D _iconAtlas;

	}
}