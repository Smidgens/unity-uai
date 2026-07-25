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
		public const int INVALID_ID = -1;

		public bool IsRunning()
		{
			return _running;
		}

		public bool IsDisposed()
		{
			return _disposed;
		}

		public IUAIAction CurrentActionTemplate => GetCurrentActionTemplate();

		public float LastBucketScoringTime => _lastBucketScoringTime;
		public float LastActionScoringTime => _lastActionScoringTime;

		// 
		public int CurrentActionID => _currentActionID;

		// 
		public int CurrentBucketID => _currentBucketID;

		public int TotalActivations => _totalActivations;
		
		// 
		public int BucketCount => _bucketRecords.Length;
		public int TotalActionCount => _actionRecords.Length;

		public UAIMemory GetMemory() => _memory;

		public UAIAgentContext GetContext() => _context;

		public int GetCurrentBucketActionCount()
		{
			return IsValidBucketID(_currentBucketID)
			? _bucketRecords[_currentBucketID].actionCount
			: 0;
		}

		public float GetCurrentBucketScoringRate()
		{
			return IsValidBucketID(_currentBucketID)
			? _bucketRecords[_currentBucketID].bucketScoringRate
			: UAIDefaults.DEFAULT_BUCKET_SCORING_RATE;
		}

		public float GetCurrentActionScoringRate()
		{
			return IsValidBucketID(_currentBucketID)
			? _bucketRecords[_currentBucketID].actionScoringRate
			: UAIDefaults.DEFAULT_ACTION_SCORING_RATE;
		}

		// 
		public float GetBucketScoringProgress()
		{
			return Mathf.Clamp01((GetCurrentTime() - _lastBucketScoringTime) / GetCurrentBucketScoringRate());
		}

		// 
		public float GetActionScoringProgress()
		{
			return Mathf.Clamp01((GetCurrentTime() - _lastActionScoringTime) / GetCurrentActionScoringRate());
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
			if (IsDisposed())
			{
				// NOTE: May not be necessary, technically we -could- reuse the instance
				throw new UAIException("Trying to start logic on a disposed brain");
			}

			if (_running)
			{
				throw new UAIException("Trying to start logic on an already running brain");
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
			_disposed = true;
			// GC cleanup if necessary
		}

		public bool IsValidActionID(int actionID) => _actionRecords.IsValidIndex(actionID);

		public bool IsValidBucketID(int bucketID) => _bucketRecords.IsValidIndex(bucketID);
		
		internal ref readonly ActionRecord GetCurrentActionRef()
		{
			return ref GetActionRef(_currentActionID);
		}

		internal ref readonly BucketRecord GetCurrentBucketRef()
		{
			return ref GetBucketRef(_currentBucketID);
		}
		
		internal ref readonly BucketRecord GetBucketRef(int bucketID)
		{
			return ref IsValidActionID(bucketID)
			? ref _bucketRecords[bucketID]
			: ref BucketRecord.Default;
		}

		internal ref readonly ActionRecord GetActionRef(int actionID)
		{
			return ref IsValidActionID(actionID)
			? ref _actionRecords[actionID]
			: ref ActionRecord.Default;
		}

		internal static UAIBrain CreateBrain(in UAIBrainInitConfig config)
		{
			var brain = new UAIBrain();
			brain._contextAgent = config.agent;
			brain._behaviour = config.behaviourTemplate;
			brain._bucketSelector = brain._behaviour._bucketSelector ?? UAIDefaults.DefaultBucketSelector;
			return brain;
		}

		internal void ForEachActionInBucket(int bucketID, ActionRefRO<ActionRecord> fn, bool sortByScore = true)
		{
			if (!_bucketRecords.IsValidIndex(bucketID))
			{
				return;
			}

			ref readonly BucketRecord bucket = ref _bucketRecords[bucketID];

			if (!sortByScore)
			{
				for (int i = 0; i < bucket.actionCount; i++)
				{
					ref readonly ActionRecord aRecord = ref _actionRecords[bucket.actionIndex + i];
					fn.Invoke(aRecord);
				}
				return;
			}

			for (int i = 0; i < bucket.actionCount; i++)
			{
				int ActionID = _actionIndicesByScore[bucket.actionIndex + i];
				ref readonly ActionRecord aRecord = ref _actionRecords[ActionID];
				fn.Invoke(aRecord);
			}
		}

		internal void ForEachBucket(ActionRefRO<BucketRecord> fn, bool sortByScore = true)
		{
			if (!sortByScore)
			{
				for (int i = 0; i < _bucketRecords.Length; i++)
				{
					ref readonly BucketRecord record = ref _bucketRecords[i];
					fn.Invoke(record);
				}
				return;
			}

			for (int i = 0; i < _bucketIndicesByScore.Length; i++)
			{
				int bucketID = _bucketIndicesByScore[i];
				ref readonly BucketRecord record = ref _bucketRecords[bucketID];
				fn.Invoke(record);
			}
		}

		internal int GetActiveServiceCount()
		{
			if (IsValidBucketID(_currentBucketID))
			{
				return _bucketRecords[_currentBucketID].services.Length;
			}
			return 0;
		}

		private Coroutine _actionScoringRoutine;
		private Coroutine _bucketScoringRoutine;
		private UAIAgentContext _context;
		private ActionRecord[] _actionRecords = Array.Empty<ActionRecord>();
		private BucketRecord[] _bucketRecords =  Array.Empty<BucketRecord>();
		private UAIManager _cachedManager;
		private int[] _actionIndicesByScore = Array.Empty<int>();
		private int[] _bucketIndicesByScore = Array.Empty<int>();
		private float _lastBucketScoringTime;
		private float _lastActionScoringTime;
		private int _currentBucketID = INVALID_ID; // index to bucket array
		private int _currentActionID = INVALID_ID; // index to action array
		private UAISelector _bucketSelector = UAIDefaults.DefaultBucketSelector;
		private IUAIAgent _contextAgent;
		private UAIMemory _memory;
		private UAIBehaviour _behaviour;
		private bool _disposed;
		private bool _deactivatingAction;
		private bool _running;
		private int _totalActivations;

		internal UAIBrainDebugContext _debugContext;

		private UAIBrain()
		{
			_memory = new ();
			_debugContext = new();
		}

		internal struct ActionRecord
		{
			public static readonly ActionRecord Default = new ActionRecord
			{
				ID = INVALID_ID,
				bucketID = INVALID_ID
			};
			public readonly bool IsValid() => ID != INVALID_ID;
			public int ID;
			public int bucketID;
			public float score;
			public double scoreSum;
			public float cooldownEnd;
			public UAIAction template;
			public UAIAction instance;
			public Coroutine activationRoutine;
			public UAIConsideration[] considerations;
			public bool cancelled;
			public bool deactivating;
			public bool cancellable;
			public float lastActivation;
			public int considerationIndex;
			public int considerationCount; // # evaluated considerations
			public bool reusable;
			public int activations;
			public float totalTimeActive;

			public readonly float GetTotalActiveTime()
			{
				if (activationRoutine != null || deactivating)
				{
					// return totalTimeActive + TimeFromLastActivation();
				}
				return totalTimeActive;
				// return totalTimeActive + TimeFromLastActivation();
			}
			
			public readonly bool OnCooldown()
			{
				if (activationRoutine != null || deactivating)
				{
					return false;
				}
				return cooldownEnd > Time.time;
			}

			public readonly float SustainedScore()
			{
				if (template._sustainAction)
				{
					var t = template._sustainCurve.Evaluate(TimeFromLastActivation());
					return Mathf.Max(0f, t * score);
				}
				return score;
			}

			public readonly float TimeFromLastActivation()
			{
				return Mathf.Max(0f, Time.time - lastActivation);
			}
		}

		internal struct BucketRecord
		{
			public static readonly BucketRecord Default = new BucketRecord
			{
				ID = INVALID_ID
			};
			public readonly bool IsValid() => ID != INVALID_ID;
			public int ID; // unique, index
			public float score; // last computed score
			public string label; // 
			public int actionIndex;
			public int actionCount;
			public float actionScoringRate;
			public float bucketScoringRate;
			public float lastActivation;
			public float weight;
			public UAISelector actionSelector; 
			internal UAIBucket bucketSO;
			internal UAIConsideration[] considerations;
			internal int considerationIndex;
			internal int considerationCount; // # evaluated considerations
			internal IUAIService[] services;
		}

		internal struct ConsiderationInfo
		{
			public UAIConsideration consideration;
			public float score;
		}

		private void InitExecutionContext()
		{
			List<BucketRecord> buckets = new();
			List<ActionRecord> actions = new();
			List<int> bucketIndices = new();
			List<int> actionIndices = new();

			int totalConsiderations = 0;

			foreach(var bucketConfig in _behaviour._buckets)
			{
				var bucketSO = bucketConfig.bucket;
				var bucketWeight = bucketConfig.enableWeight
				? bucketConfig.overrideWeight
				: bucketConfig.bucket._weight;
				
				var bucketConsiderations = bucketConfig.enableConsiderations
				? bucketConfig.overrideConsiderations._considerations.GetItems()
				: bucketConfig.bucket._bucketConsiderations.GetItems();

				// filter out invalids
				bucketConsiderations = bucketConsiderations
				.Where(c => c != null && c.Enabled)
				.ToArray();

				var actionSelector = bucketConfig.enableSelector && bucketConfig.overrideSelector != null
				? bucketConfig.overrideSelector
				: bucketConfig.bucket._actionSelector;
				actionSelector = actionSelector ?? UAIDefaults.DefaultActionSelector;

				var services = bucketSO._services.GetItems()
				.Where(x => x != null)
				.Select(x => x.Clone(this))
				.ToList();

				var externalServices = bucketSO._externalServices
				.Where(x => x != null)
				.Select(x => x.Clone(this));
				
				services.AddRange(externalServices);

				foreach (var s in services)
				{
					s.InitService();
				}

				BucketRecord bucketRecord = new BucketRecord
				{
					ID = buckets.Count,
					label = bucketSO.BucketName,
					actionIndex = actions.Count,
					bucketSO = bucketSO,
					actionScoringRate = bucketSO._actionScoringRate,
					bucketScoringRate = bucketSO._bucketScoringRate,
					weight = bucketWeight,
					considerations = bucketConsiderations,
					considerationIndex = totalConsiderations,
					actionSelector = actionSelector,
					actionCount = 0,
					services = services.ToArray(),
				};

				// track consideration
				totalConsiderations += bucketRecord.considerations.Length;

				foreach (var action in bucketSO._actions.GetItems())
				{
					if (!action || !action.Enabled)
					{
						continue;
					}
					bucketRecord.actionCount++;

					var actionTemplate = action.InstantiateAction();

					var actionConsiderations = action._considerations.GetItems()
					.Where(c => c && c.Enabled)
					.ToArray();

					var actionRecord = new ActionRecord
					{
						bucketID = bucketRecord.ID,
						ID = actions.Count,
						template = actionTemplate,
						reusable = actionTemplate.IsReusable(),
						considerations = actionConsiderations,
						considerationIndex = totalConsiderations
					};

					totalConsiderations += actionConsiderations.Length;
					actions.Add(actionRecord);
					actionIndices.Add(actionIndices.Count);
				}
				buckets.Add(bucketRecord);
				bucketIndices.Add(bucketIndices.Count);
			}

			_debugContext.considerationScores = new float [totalConsiderations];

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
			_lastBucketScoringTime = GetCurrentTime();
			for (int i = 0; i < _bucketRecords.Length; i++)
			{
				int bucketID = i;
				ref BucketRecord record = ref _bucketRecords[bucketID];
				record.score = GetBucketScore(record, out var count);
				record.considerationCount = count;
			}

			UAISort.IndicesByWeight(ref _bucketIndicesByScore, 0, _bucketIndicesByScore.Length, i =>
			{
				return _bucketRecords[i].score;
			}, false);
		}

		internal string GetCurrentBucketLabel()
		{
			if (!IsValidBucketID(_currentBucketID))
			{
				return string.Empty;
			}
			return _bucketRecords[_currentBucketID].label;
		}

		// 
		private void ScoreActions()
		{
			_lastActionScoringTime = GetCurrentTime();

			if (!IsValidBucketID(_currentBucketID))
			{
				return;
			}

			ref readonly BucketRecord bucket = ref _bucketRecords[_currentBucketID];
			
			var scoreCtx = new UAIScoringContext
			{
				scores = _debugContext.considerationScores
			};

			for (int i = 0; i < bucket.actionCount; i++)
			{
				var actionID = _actionIndicesByScore[bucket.actionIndex + i];
				ref ActionRecord record = ref _actionRecords[actionID];

				scoreCtx.scoreIndex = record.considerationIndex;
				record.cancellable = record.instance != null ? record.instance.CanCancelAction() : false;

				bool shouldScore = true;

				if (actionID == _currentActionID && record.template._sustainAction)
				{
					shouldScore = false;
				}

				if (shouldScore)
				{
					record.score = GetActionScore(record, _context, scoreCtx, out int count);
					record.considerationCount = count;
				}
			}

			UAISort.IndicesByWeight(ref _actionIndicesByScore, bucket.actionIndex, bucket.actionCount, i =>
			{
				// return _actionRecords[i].score;
				return _actionRecords[i].SustainedScore();
			}, false);
		}

		private void SetNextBucket()
		{
			var lastBucketID = _currentBucketID;
			_currentBucketID = SelectBucket();

			var changed = lastBucketID != _currentBucketID;

			if (IsValidBucketID(lastBucketID) && changed)
			{
				foreach (var s in _bucketRecords[lastBucketID].services)
				{
					s.StopService();
				}
			}
			
			if (IsValidBucketID(_currentBucketID) && changed)
			{
				_bucketRecords[_currentBucketID].lastActivation = GetCurrentTime();
				
				foreach (var s in _bucketRecords[_currentBucketID].services)
				{
					s.StartService();
				}
			}
			
		}

		private float GetCurrentTime()
		{
			return Time.time;
		}

		private void ResetAction()
		{
			_currentActionID = INVALID_ID;
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
			if (IsValidActionID(nextIndex) && nextIndex == _currentActionID)
			{
				return;
			}

			if (_actionRecords.IsValidIndex(_currentActionID))
			{
				ref readonly ActionRecord action = ref _actionRecords[_currentActionID];

				if (!action.deactivating)
				{
					CancelAction(action.ID, ResetAction);
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
			
			return scoreIndex > -1 ? _bucketIndicesByScore[scoreIndex] : INVALID_ID;
		}

		// 
		private int SelectAction()
		{
			if (!IsValidBucketID(_currentBucketID))
			{
				return INVALID_ID;
			}

			ref readonly BucketRecord bucket = ref _bucketRecords[_currentBucketID];
			int aIndex = bucket.actionIndex;

			var selector = bucket.bucketSO._actionSelector;

			int scoreIndex = bucket.actionIndex + selector.SelectIndex(bucket.actionCount, i =>
			{
				ref readonly ActionRecord action = ref _actionRecords[_actionIndicesByScore[aIndex + i]];
				return action.OnCooldown() ? 0f : action.SustainedScore();
			});
			return scoreIndex > -1 ? _actionIndicesByScore[scoreIndex] : INVALID_ID;
		}

		// get last score
		private float GetActionScore(in int actionID)
		{
			ref readonly var aRef = ref GetActionRef(actionID);
			return aRef.score;
		}

		// compute score and return it 
		private float GetActionScore(in ActionRecord record, in UAIAgentContext context, in UAIScoringContext scoreContext, out int count)
		{
			count = 0;
			float weight = record.template._weight;
			if (Mathf.Approximately(weight, 0f))
			{
				return 0f;
			}

			if (record.considerations.Length == 0)
			{
				return weight * UAIDefaults.DEFAULT_ACTION_SCORE;
			}
			var score = UAIMath.ScoreConsiderations(context, record.considerations, out count, scoreContext);
			return weight * score;
		}

		private float GetBucketScore(in BucketRecord bucket, out int count)
		{
			count = 0;
			var scoreCtx = new UAIScoringContext
			{
				scoreIndex = bucket.considerationIndex,
				scores = _debugContext.considerationScores
			};

			var score = UAIMath.ScoreConsiderations(_context, bucket.considerations, out count, scoreCtx);
			return count > 0 ? score * bucket.weight : bucket.weight;
		}

		private void CancelAction(int actionID, Action onDone)
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
			var lastActivation = record.lastActivation;
			UAIManager.RunCoroutine(DeactivateActionRoutine(actionID), onDone);
			record.totalTimeActive += Mathf.Max(0f, Time.time - lastActivation);
		}

		internal int GetActiveConsiderationCount()
		{
			var count = 0;

			if (IsValidBucketID(_currentBucketID))
			{
				count += _bucketRecords[_currentBucketID].considerationCount;
			}
			
			if (IsValidActionID(_currentActionID))
			{
				count += _actionRecords[_currentActionID].considerationCount;
			}

			return count;
		}

		internal void ForEachActiveBucketConsideration(ActionRefRO<ConsiderationInfo> fn)
		{
			if (!IsValidBucketID(_currentBucketID))
			{
				return;
			}

			ref readonly BucketRecord bucket = ref _bucketRecords[_currentBucketID];

			for (int i = 0; i < bucket.considerationCount; i++)
			{
				var consideration = bucket.considerations[i];
				var score = _debugContext.considerationScores[bucket.considerationIndex + i];

				fn.Invoke(new ConsiderationInfo
				{
					consideration = consideration,
					score = score,
				});
			}
		}
		
		internal void ForEachActiveActionConsideration(ActionRefRO<ConsiderationInfo> fn)
		{
			if (!IsValidActionID(_currentActionID))
			{
				return;
			}

			ref ActionRecord action = ref _actionRecords[_currentActionID];
			
			for (int i = 0; i < action.considerationCount; i++)
			{
				var consideration = action.considerations[i];
				var score = _debugContext.considerationScores[action.considerationIndex + i];

				fn.Invoke(new ConsiderationInfo
				{
					consideration = consideration,
					score = score,
				});
			}
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
				action.cooldownEnd = GetCurrentTime() + instance.GetActionCooldown();
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

			if (!record.instance || !record.reusable)
			{
				record.instance = record.template.InstantiateAction();
			}
			record.instance._brain = this;
			record.instance._status = EUAIActionStatus.Active;
			record.activations++;
			record.lastActivation = GetCurrentTime();
			record.scoreSum += record.score;
			_totalActivations++;
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

			if (record.instance && record.reusable)
			{
				return;
			}

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
				yield return new WaitUntil(PRED_NotDeactivatingAction);
				ScoreActions();
				SetNextAction();
				yield return new WaitForSeconds(GetCurrentActionScoringRate());
			}
		}

		private IEnumerator BucketScoringRoutine()
		{
			while (true)
			{
				yield return new WaitUntil(PRED_NotDeactivatingAction);
				ScoreBuckets();
				SetNextBucket();
				yield return new WaitForSeconds(GetCurrentBucketScoringRate());
			}
		}

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
		
		// predicate for coroutines
		private bool PRED_NotDeactivatingAction() => !_deactivatingAction;

		private static void StartRoutine(ref Coroutine outRef, Func<IEnumerator> fn)
		{
			outRef = UAIManager.RunCoroutine(fn());
		}

	}
}