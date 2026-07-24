// smidgens @ github

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.Collections.Generic;

	// base class for most scriptable objects
	[ExcludeFromPreset]
	public abstract class UAIScriptableObject : ScriptableObject
	{
		public string Name => _label;

		public bool Enabled => _enabled;

		[SerializeField] internal string _label = "";
		
		[SOArrayColumn(18f)]
		[HideInInspector]
		[SerializeField] internal bool _enabled = true;

		[HideInInspector]
		[UnityEngine.Serialization.FormerlySerializedAs("_internalID")]
		[SerializeField] internal string _id = System.Guid.NewGuid().ToString().Replace("-", "");

		// tells inspector which assets are "owned" by this object
		// in case it's nested
		internal virtual void GatherNestedAssets(List<UAIScriptableObject> assets)
		{
		
		}
	}
}


#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI.Editor
{
	using UnityEditor;

	internal abstract class _UAIScriptableObject : Editor
	{
		protected override bool ShouldHideOpenButton() => true;
	}
}

#endif