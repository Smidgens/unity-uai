// smidgens @ github

// ReSharper disable All

using System;

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System.Collections.Generic;

	// base class for all scriptable objects
	[ExcludeFromPreset]
	public abstract class UtilityAISO : ScriptableObject
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
		internal virtual void GatherNestedAssets(List<UtilityAISO> assets)
		{
		
		}
	}
}



#if UNITY_EDITOR

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using UnityEditor;
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using UObject = UnityEngine.Object;

	// partial class UtilityAISO
	// {
	// 	protected virtual void DrawListGUI(ref Rect pos)
	// 	{
	// 		// custom item gui here
	// 	}
	// 	
	//
	// }
}

#endif