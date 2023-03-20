using Fusion;
using UnityEngine;

namespace Animations
{
	public class AnimatorStateSync : NetworkBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private Animator _animator;
		[SerializeField]
		private bool _enableAutoSync;
		[SerializeField, Tooltip("Sync interval in seconds")]
		private float _autoSyncInterval = 2f;

		[Networked]
		private int _syncTick { get; set; }
		[Networked, Capacity(12)]
		private NetworkArray<StateData> _states { get; }

		private Interpolator<int> _syncTickInterpolator;
		private RawInterpolator _statesInterpolator;

		private int _lastVisibleSyncTick;
		private bool _layerSyncPending;

		private int _layerCount = -1;

		// PUBLIC METHODS

		public void RequestSync()
		{
			if (HasStateAuthority == false)
				return;

			if (_animator == null)
				return;

			_layerSyncPending = true;

			SynchronizeStates();
		}

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			_statesInterpolator = GetInterpolator(nameof(_states));
			_syncTickInterpolator = GetInterpolator<int>(nameof(_syncTick));
		}

		public override void FixedUpdateNetwork()
		{
			if (HasStateAuthority == false)
				return;

			UpdateAutoSync();

			if (_layerSyncPending == true)
			{
				SynchronizeStates();
			}
		}

		public override void Render()
		{
			if (Object.IsProxy == true)
			{
				UpdateStates();
			}
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			_layerCount = _animator != null ? _animator.layerCount : 0;
		}

		// PRIVATE METHODS

		private void UpdateAutoSync()
		{
			if (_enableAutoSync == false)
				return;

			if (Runner.SimulationTime > _syncTick * Runner.DeltaTime + _autoSyncInterval)
			{
				RequestSync();
			}
		}

		private void SynchronizeStates()
		{
			for (int i = 0; i < _layerCount; i++)
			{
				if (_animator.IsInTransition(i) == true)
					return; // Wait until all animator layers are out of transition
			}

			for (int i = 0; i < _layerCount; i++)
			{
				var stateInfo = _animator.GetCurrentAnimatorStateInfo(i);

				// Use only fractional part
				// (integer part is the number of times a state has been looped)
				float time = stateInfo.normalizedTime % 1;

				_states.Set(i, new StateData(stateInfo.fullPathHash, time));
			}

			_syncTick = Runner.Simulation.Tick;
			_layerSyncPending = false;
		}

		private void UpdateStates()
		{
			_syncTickInterpolator.TryGetValues(out int syncTickFrom, out int syncTickTo, out float syncTickAlpha);

			if (_lastVisibleSyncTick == syncTickFrom)
				return;

			_statesInterpolator.TryGetArray(_states, out var fromStates, out var toStates, out float alpha);
			_lastVisibleSyncTick = syncTickFrom;

			for (int i = 0; i < _layerCount; i++)
			{
				var fromState = fromStates.Get(i);
				var toState = toStates.Get(i);

				int stateHash = alpha < 0.5f ? fromState.StateHash : toState.StateHash;

				if (stateHash != 0)
				{
					bool stateChanged = fromState.StateHash != toState.StateHash;

					float time = InterpolateTime(fromState.NormalizedTime, toState.NormalizedTime, alpha, stateChanged);
					_animator.Play(stateHash, i, time);
				}
			}
		}

		private static float InterpolateTime(float from, float to, float alpha, bool stateChanged)
		{
			if (to >= from)
				return Mathf.Lerp(from, to, alpha);

			float time = Mathf.Lerp(from, to + 1f, alpha);

			// Make sure time is not larger than 1
			time = time > 1f ? time - 1f : time;

			// It is possible that states could be switched too soon due to the simple
			// rounding to either FromState or ToState based on the alpha < 0.5
			if (stateChanged == true && time > to)
				return 0f; // Wait with next state until alpha will be large enough

			if (stateChanged == false && time < from)
				return 1f; // Continue with previous state until alpha will be large enough

			return time;
		}

		// DATA STRUCTURES

		public struct StateData : INetworkStruct
		{
			public readonly int StateHash;
			[Networked, Accuracy(0.01f)]
			public float NormalizedTime { get; private set; }

			public StateData(int stateHash, float normalizedTime)
			{
				StateHash = stateHash;
				NormalizedTime = normalizedTime;
			}
		}
	}
}
