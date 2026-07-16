// smidgens @ github

// ReSharper disable All

#pragma warning disable 0414
#pragma warning disable 0067

namespace Smidgenomics.Unity.UAI
{
	using System;
	using UnityEngine;
	using System.Collections.Generic;
	using System.Linq;
	using IEnumerator = System.Collections.IEnumerator;

	public sealed class UAIBrain
	{
		public IUAIAction CurrentTemplate => GetCurrentActionTemplate();

		public UAIMemory GetMemory() => _memory;

		public UAIAgentContext GetContext() => _context;

		// 
		public int GetCurrentActionID() => _currentActionID;

		// 
		public int GetCurrentBucketID() => _currentBucketID;

		public int GetCurrentBucketActionCount()
		{
			return IsValidBucketID(_currentBucketID) ? _bucketRecords[_currentBucketID].actionCount : 0;
		}

		// 
		internal ref readonly ActionRecord GetCurrentAction() => ref _actionRecords[_currentActionID];
	
		// 
		public int GetBucketCount() => _bucketRecords.Length;

		public float GetCurrentBucketScoringRate()
		{
			return IsValidBucketID(_currentBucketID)
			? _bucketRecords[_currentBucketID].bucketScoringRate
			: _defaultBucketScoringRate;
		}

		public float GetCurrentActionScoringRate()
		{
			return IsValidBucketID(_currentBucketID)
			? _bucketRecords[_currentBucketID].actionScoringRate
			: _defaultActionScoringRate;
		}

		// 
		public float GetBucketScoringProgress()
		{
			return Mathf.Clamp01((Time.time - _lastBucketScoringTime) / GetCurrentBucketScoringRate());
		}

		// 
		public float GetActionScoringProgress()
		{
			return Mathf.Clamp01((Time.time - _lastActionScoringTime) / GetCurrentActionScoringRate());
		}

		public int GetCurrentActionBucketID()
		{
			return IsValidActionID(_currentActionID) ? _actionRecords[_currentActionID].bucketID : -1;
		}

		public UAISelector GetCurrentBucketSelector()
		{
			return _bucketSelector;
		}

		public UAISelector GetCurrentActionSelector()
		{
			return IsValidBucketID(_currentBucketID)
			? _bucketRecords[_currentBucketID].actionSelector
			: UAIDefaults.DefaultActionSelector;
		}

		public void StartLogic()
		{
			if (_running)
			{
				return;
			}
			
			if (_behaviour == null)
			{
				return;
			}

			_cachedManager = UAIManager.GetInstance();
			_cachedManager.RegisterBrain(this);
			_running = true;

			_context = new UAIAgentContext
			{
				agent = _contextAgent,
				memory = _memory
			};

			InitExecutionContext();
		}

		public void StopLogic()
		{
			if (!_running)
			{
				return;
			}

			_memory.ClearAllValues();

			_running = false;

			if (_cachedManager)
			{
				_cachedManager.UnregisterBrain(this);
				UAIManager.StopRoutine(_actionScoringRoutine);
				UAIManager.StopRoutine(_bucketScoringRoutine);
			}
			_actionScoringRoutine = null;
		}
	
		/// <summary>
		/// Clean up spawned objects
		/// </summary>
		public void Dispose()
		{
			// GC cleanup if necessary
		}

		public bool IsValidActionID(int actionID) => _actionRecords.IsValidIndex(actionID);

		public bool IsValidBucketID(int bucketID) => _bucketRecords.IsValidIndex(bucketID);

		internal static UAIBrain CreateBrain(in UAIBrainInitConfig config)
		{
			var brain = new UAIBrain();
			brain._contextAgent = config.agent;
			brain._behaviour = config.behaviourTemplate;
			brain._bucketSelector = brain._behaviour._bucketSelector ?? UAIDefaults.DefaultBucketSelector;
			return brain;
		}

		internal void ForEachActionInBucket(int bucketID, ActionRefRO<ActionRecord> fn)
		{
			if (!_bucketRecords.IsValidIndex(bucketID))
			{
				return;
			}

			ref readonly BucketRecord bucket = ref _bucketRecords[bucketID];

			for (int i = 0; i < bucket.actionCount; i++)
			{
				int ActionID = _actionIndicesByScore[bucket.actionIndex + i];
				ref readonly ActionRecord aRecord = ref _actionRecords[ActionID];
				fn.Invoke(aRecord);
			}
		}

		internal void ForEachBucket(ActionRefRO<BucketRecord> fn)
		{
			for (int i = 0; i < _bucketIndicesByScore.Length; i++)
			{
				int bucketID = _bucketIndicesByScore[i];
				ref readonly BucketRecord record = ref _bucketRecords[bucketID];
				fn.Invoke(record);
			}
		}

		private Coroutine _actionScoringRoutine;
		private Coroutine _bucketScoringRoutine;
		private UAIAgentContext _context;
		internal ActionRecord[] _actionRecords = Array.Empty<ActionRecord>();
		private BucketRecord[] _bucketRecords =  Array.Empty<BucketRecord>();
		private UAIManager _cachedManager;
		private bool _deactivatingAction;
		private bool _running;
		private int[] _actionIndicesByScore = Array.Empty<int>();
		private int[] _bucketIndicesByScore = Array.Empty<int>();
		private float _lastBucketScoringTime;
		private float _lastActionScoringTime;
		private int _currentBucketID = -1;
		private int _currentActionID = -1;
		private float _defaultActionScoringRate = 1f;
		private float _defaultBucketScoringRate = 5f;
		private UAISelector _bucketSelector = UAIDefaults.DefaultBucketSelector;
		private IUAIAgent _contextAgent;
		private UAIMemory _memory;
		private UAIBehaviour _behaviour;

		private UAIBrain()
		{
			_memory = new UAIMemory();
		}

		internal struct ActionRecord
		{
			public int actionID;
			public int bucketID;
			public float score;
			public float activationScore;
			public float cooldownEnd;
			public UAIAction template;
			public UAIAction instance;
			public Coroutine activationRoutine;
			public bool cancelled;
			public bool deactivating;
			public bool cancellable;

			public bool OnCooldown()
			{
				if (instance != null)
				{
					return false;
				}
				return cooldownEnd > Time.time;
			}
			public EUAIActionStatus Status => instance != null ? instance.GetActionStatus() : EUAIActionStatus.Inactive;
		}

		internal struct BucketRecord
		{
			public int ID;
			public float score;
			public string name;
			public int actionIndex;
			public int actionCount;
			public float actionScoringRate;
			public float bucketScoringRate;
			public float weight;
			public UAISelector actionSelector; 
			public UAIBucket bucketSO;
			public UAIConsideration[] considerations;
		}

		private void InitExecutionContext()
		{
			List<BucketRecord> buckets = new();
			List<ActionRecord> actions = new();
			List<int> bucketIndices = new();
			List<int> actionIndices = new();

			foreach(var bucketConfig in _behaviour._buckets)
			{
				var bucketSO = bucketConfig.bucket;

				BucketRecord bucketRecord = new BucketRecord();
				bucketRecord.ID = buckets.Count;
				bucketRecord.name = bucketSO.BucketName;
				bucketRecord.actionIndex = actions.Count;
				bucketRecord.bucketSO = bucketSO;
				
				bucketRecord.actionScoringRate = bucketSO._actionScoringRate;
				bucketRecord.bucketScoringRate = bucketSO._bucketScoringRate;

				bucketRecord.considerations = bucketConfig.enableConsiderations
				? bucketConfig.overrideConsiderations._considerations.GetItems()
				: bucketConfig.bucket._bucketConsiderations.GetItems();

				bucketRecord.weight = bucketConfig.enableWeight
				? bucketConfig.overrideWeight
				: bucketConfig.bucket._weight;

				bucketRecord.actionSelector = bucketConfig.enableSelector && bucketConfig.overrideSelector != null
				? bucketConfig.overrideSelector
				: bucketConfig.bucket._actionSelector;
				bucketRecord.actionSelector = bucketRecord.actionSelector ?? UAIDefaults.DefaultActionSelector;

				int aCount = 0;

				foreach (var action in bucketSO._actions.GetItems())
				{
					if (!action.Enabled)
					{
						continue;
					}
					aCount++;
					var record = new ActionRecord();
					record.actionID = actions.Count;
					record.template = action.InstantiateAction();
					record.bucketID = bucketRecord.ID;
					actions.Add(record);
					actionIndices.Add(actionIndices.Count);
				}
				bucketRecord.actionCount = aCount;
				buckets.Add(bucketRecord);
				bucketIndices.Add(bucketIndices.Count);
			}

			// _currentBucketID = buckets.Count > 0 ? 0 : -1;
			_actionRecords = actions.ToArray();
			_bucketRecords = buckets.ToArray();
			_actionIndicesByScore = actionIndices.ToArray();
			_bucketIndicesByScore = bucketIndices.ToArray();

			SetNextBucket();
			SetNextAction();

			StartRoutine(ref _bucketScoringRoutine, BucketScoringRoutine);
			StartRoutine(ref _actionScoringRoutine, ActionScoringRoutine);

			if (!_cachedManager)
			{
				_cachedManager = UAIManager.GetInstance();
			}
		}

		// 
		private void ScoreBuckets()
		{
			_lastBucketScoringTime = Time.time;
			for (int i = 0; i < _bucketRecords.Length; i++)
			{
				int bucketID = i;
				ref BucketRecord record = ref _bucketRecords[bucketID];
				record.score = GetBucketScore(record);
			}

			UAIHelpers.SortIndicesByWeight(ref _bucketIndicesByScore, 0, _bucketIndicesByScore.Length, i =>
			{
				return _bucketRecords[i].score;
			}, false);

		}

		// 
		private void ScoreActions()
		{
			_lastActionScoringTime = Time.time;

			if (!IsValidBucketID(_currentBucketID))
			{
				return;
			}

			ref readonly BucketRecord bucket = ref _bucketRecords[_currentBucketID];

			for (int i = 0; i < bucket.actionCount; i++)
			{
				var actionID = _actionIndicesByScore[bucket.actionIndex + i];
				ref ActionRecord record = ref _actionRecords[actionID];
				record.score = record.template.GetTotalScore(_context);
				record.cancellable = record.instance != null ? record.instance.CanCancelAction() : false;
			}

			UAIHelpers.SortIndicesByWeight(ref _actionIndicesByScore, bucket.actionIndex, bucket.actionCount, i =>
			{
				return _actionRecords[i].score;
			}, false);
		}

		private void SetNextBucket()
		{
			_currentBucketID = SelectBucket();
		}

		private void ResetAction()
		{
			_currentActionID = -1;
			SetNextAction();
		}

		private void SetNextAction()
		{
			if (_deactivatingAction)
			{
				return;
			}

			// action is active and uncancellable
			if (IsValidActionID(_currentActionID) && !_actionRecords[_currentActionID].cancellable)
			{
				return;
			}

			if (!IsValidBucketID(_currentBucketID))
			{
				return;
			}

			ref readonly BucketRecord currBucket = ref _bucketRecords[_currentBucketID];

			var nextIndex = SelectAction();

			// already running best action
			if (nextIndex >= 0 && nextIndex == _currentActionID)
			{
				return;
			}

			if (_actionRecords.IsValidIndex(_currentActionID))
			{
				ref readonly ActionRecord action = ref _actionRecords[_currentActionID];

				if (!action.deactivating)
				{
					CancelAction(action.actionID, ResetAction);
				}
			}
			else if(_actionRecords.IsValidIndex(nextIndex))
			{
				ActivateAction(nextIndex);
			}
		}

		private int SelectBucket()
		{
			int scoreIndex = _bucketSelector.SelectIndex(_bucketRecords.Length, i =>
			{
				return _bucketRecords[_bucketIndicesByScore[i]].score;
			});
			
			return scoreIndex > -1 ? _bucketIndicesByScore[scoreIndex] : -1;
		}

		// 
		private int SelectAction()
		{
			if (!IsValidBucketID(_currentBucketID))
			{
				return -1;
			}

			ref readonly BucketRecord bucket = ref _bucketRecords[_currentBucketID];
			int aIndex = bucket.actionIndex;

			int scoreIndex = bucket.actionIndex + bucket.bucketSO._actionSelector.SelectIndex(bucket.actionCount, i =>
			{
				return _actionRecords[_actionIndicesByScore[aIndex + i]].score;
			});
			return scoreIndex > -1 ? _actionIndicesByScore[scoreIndex] : -1;
		}

		private float GetActionScore(in int actionID)
		{
			return _actionRecords[actionID].score;
		}

		private float GetBucketScore(in BucketRecord bucket)
		{
			var score = UAIMath.ScoreConsiderations(_context, bucket.considerations, out var count);
			return count > 0 ? score * bucket.weight : bucket.weight;
		}

		void CancelAction(int actionID, Action onDone)
		{
			ref ActionRecord record = ref _actionRecords[actionID];
			record.cancelled = true;
			DeactivateAction(actionID, EUAIActionStatus.Cancelled, onDone);
		}

		private void DeactivateAction(int actionID, EUAIActionStatus status, Action onDone)
		{
			if (_deactivatingAction)
			{
				return;
			}

			_deactivatingAction = true;

			ref ActionRecord record = ref _actionRecords[actionID];
			
			UAIManager.StopRoutine(record.activationRoutine);
			record.activationRoutine = null;

			if (record.instance != null)
			{
				record.instance._status = status;
			}
			
			record.deactivating = true;
			UAIManager.RunCoroutine(DeactivateActionRoutine(actionID), onDone);
		}

		private IEnumerator DeactivateActionRoutine(int actionID)
		{
			var instance = _actionRecords[actionID].instance;

			if (instance != null)
			{
				yield return instance.DeactivateAction();
			}
			
			ActionRecord action = _actionRecords[actionID];

			if (instance != null)
			{
				action.cooldownEnd = Time.time + instance.GetActionCooldown();
			}
			
			action.deactivating = false;
			_actionRecords[actionID] = action;
			DisposeActionInstance(actionID);
			yield return null;

			_deactivatingAction = false;
			
			yield return null;
		}

		private void ActivateAction(int actionID)
		{
			_currentActionID = actionID;

			ref ActionRecord record = ref _actionRecords[actionID];
			record.cancelled = false;

			record.instance = record.template.InstantiateAction();
			record.instance._brain = this;

			record.instance._status = EUAIActionStatus.Active;
			record.activationRoutine = UAIManager.RunCoroutine(record.instance.ActivateAction(), OnActionFinished);
		}

		// called when action finishes early
		private void OnActionFinished()
		{
			DeactivateAction(_currentActionID, EUAIActionStatus.Finished, ResetAction);
		}

		private void DisposeActionInstance(int actionID)
		{
			ref ActionRecord record = ref _actionRecords[actionID];

			if (record.instance != null)
			{
				if (record.instance.GetType().IsSubclassOf(typeof(UnityEngine.Object)))
				{
					UnityEngine.Object.Destroy(record.instance as UnityEngine.Object);
				}
			}
			record.instance = null;
		}

		private IEnumerator ActionScoringRoutine()
		{
			while (true)
			{
				
				yield return new WaitUntil(NotDeactivatingAction);
				ScoreActions();
				SetNextAction();
				yield return new WaitForSeconds(GetCurrentActionScoringRate());
			}
		}

		private IEnumerator BucketScoringRoutine()
		{
			while (true)
			{
				yield return new WaitUntil(NotDeactivatingAction);
				ScoreBuckets();
				SetNextBucket();
				yield return new WaitForSeconds(GetCurrentBucketScoringRate());
			}
		}

		private bool NotDeactivatingAction() => !_deactivatingAction;

		private IUAIAction GetCurrentActionInstance()
		{
			return _actionRecords.IsValidIndex(_currentActionID)
			? _actionRecords[_currentActionID].instance
			: null;
		}

		private IUAIAction GetCurrentActionTemplate()
		{
			return _actionRecords.IsValidIndex(_currentActionID)
			? _actionRecords[_currentActionID].template
			: null;
		}

		private static void StartRoutine(ref Coroutine outRef, Func<IEnumerator> fn)
		{
			outRef = UAIManager.RunCoroutine(fn());
		}

	}
}