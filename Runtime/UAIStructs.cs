// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	// TODO: Consider if this can be templated or extended (custom context payload)
	public struct UtilityAIContext
	{
		// target gameobject
		public GameObject gameObject;
		
		public UtilityAIMemory memory;
	}
}

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;

	public ref struct UtilityBrainInitConfig
	{
		public GameObject gameObject;
		public UtilityAIBehaviour behaviourDefinition;
	}
}

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;

	[System.Serializable]
	public struct UtilityAIMemoryValue
	{
		public object Object { get; set; }
		public int Integer { get; set; }
		public float Float { get; set; }
		public bool Boolean { get; set; }
		public string String { get; set; }
		public Vector3 Vector { get; set; }
	}
}

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;

	[System.Serializable]
	internal struct FloatInterval
	{
		public float min, max;
	}
}