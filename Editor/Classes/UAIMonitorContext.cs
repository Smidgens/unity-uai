// smidgens @ github

// resharper disable all

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEngine;
	using System;

	internal sealed class UAIMonitorContext
	{
		public UAIMonitorContext()
		{
			_styles = new(UAIEditorStyles.CreateInstance);
			// _iconAtlas = UAIEditorAtlas.Create();
		}

		public UAIEditorStyles Styles => _styles.Value;
		public UAIEditorAtlas IconAtlas => _iconAtlas;

		private Lazy<UAIEditorStyles> _styles;
		private UAIEditorAtlas _iconAtlas;

	}
}