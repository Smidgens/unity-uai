// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	// TODO: Consider if this can be templated or extended (custom context payload)
	public struct UAIAgentContext
	{
		// root object
		public IUAIAgent agent;
		public UAIMemory memory;
	}
}

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;

	public ref struct UAIBrainInitConfig
	{
		public IUAIAgent agent;
		public UAIBehaviour behaviourTemplate;
	}
}

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;

	// c++ unions are nice...
	[System.Serializable]
	public struct UAIMemoryValue
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
	internal struct UAIFloatRange
	{
		public float min, max;
	}
}