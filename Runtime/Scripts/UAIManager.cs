// smidgens @ github

using System.Collections.Generic;
using System.Linq;

#pragma warning disable 0414

namespace Smidgenomics.Unity.UAI
{
	using UnityEngine;
	using System;
	using Routine = System.Collections.IEnumerator;

	// spawned by utility brain to run things like coroutines
	[AddComponentMenu("")]
	[DefaultExecutionOrder(-2)]
	internal sealed class UAIManager : MonoBehaviour
	{
		public int BrainCount => _brains.Length;

		public static UAIManager GetInstance()
		{
			if (!_instance && !_closing)
			{
				_instance = (new GameObject(nameof(UAIManager))).AddComponent<UAIManager>();
				_instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
			}
			return _instance;
		}

		internal void RegisterBrain(UAIBrain aiBrain)
		{
			if (FindBrainIndex(aiBrain) > -1)
			{
				return;
			}

			TrackedBrain tbrain = new TrackedBrain();
			tbrain.AIBrain = aiBrain;

			var blist = _brains.ToList();
			blist.Add(tbrain);

			_brains = blist.ToArray();
		}
		
		internal void UnregisterBrain(UAIBrain aiBrain)
		{
			int index = FindBrainIndex(aiBrain);
			if (index < 0)
			{
				return;
			}
			var blist = _brains.ToList();
			blist.RemoveAt(index);
			_brains = blist.ToArray();
		}

		internal void ForEachTrackedBrain(ActionRefRO<TrackedBrain> fn)
		{
			for (int i = 0; i < _brains.Length; i++)
			{
				ref readonly TrackedBrain tbrain = ref _brains[i];
				fn.Invoke(tbrain);
			}
		}

		internal struct TrackedBrain
		{
			public UAIBrain AIBrain;
		}

		private static bool _closing;

		private TrackedBrain[] _brains = Array.Empty<TrackedBrain>();
	
		private int FindBrainIndex(UAIBrain aiBrain)
		{
			for (int i = 0; i < _brains.Length; i++)
			{
				ref readonly TrackedBrain tbrain = ref _brains[i];
				if (tbrain.AIBrain == aiBrain)
				{
					return i;
				}
			}
			return -1;
		}

		private void OnDestroy()
		{
			StopAllCoroutines();
			_instance = null;
		}

		public static void StopRoutine(Coroutine routine)
		{
			if (_instance && routine != null)
			{
				_instance.StopCoroutine(routine);
			}
		}

		public static Coroutine RunCoroutine(Routine routine, Action onDone = null)
		{
			return GetInstance().StartCoroutine(StartRoutine(routine, onDone));
		}

		internal static UAIManager _instance;

		private void OnEnable()
		{
			Application.quitting -= OnQuit;
			Application.quitting += OnQuit;
		}

		private void OnDisable()
		{
			Application.quitting -= OnQuit;
		}

		private void OnQuit()
		{
			
		}

		private static Routine StartRoutine(Routine routine, Action action)
		{
			yield return routine;
			action?.Invoke();
		}
	}
}