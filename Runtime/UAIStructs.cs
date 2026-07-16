// smidgens @ github

// ReSharper disable All

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;

	// Note: Consider if this can be templated or extended (custom context payload)
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
	using System.Collections.Generic;
	using System.Runtime.InteropServices;

	[StructLayout(LayoutKind.Explicit)]
	public struct UAIMemoryValue
	{
		[FieldOffset(0)] public object objectRef;
		[FieldOffset(0)] public int intValue;
		[FieldOffset(0)] public bool boolValue;
		[FieldOffset(0)] public float floatValue;
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