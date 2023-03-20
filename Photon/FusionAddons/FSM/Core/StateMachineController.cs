using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Fusion.FSM
{
	public interface IStateMachineOwner
	{
		void CollectStateMachines(List<IStateMachine> stateMachines);
	}

	[DisallowMultipleComponent]
	public sealed class StateMachineController : NetworkBehaviour, IBeforeAllTicks, IAfterTick
	{
		// PUBLIC MEMBERS

		public List<IStateMachine> StateMachines => _stateMachines;
		
		// PRIVATE MEMBERS

		[Header("DEBUG")]
		[SerializeField]
		private bool _enableLogging = false;

		private List<IStateMachine> _stateMachines = new List<IStateMachine>(32);
		private List<IState> _statePool;
		
		private bool _stateMachinesCollected;
		private bool _manualUpdate;

		// PUBLIC METHODS

		public void SetManualUpdate(bool manualUpdate)
		{
			_manualUpdate = manualUpdate;
		}

		public void ManualFixedUpdate()
		{
			if (_manualUpdate == false)
				throw new InvalidOperationException("Manual update is not turned on");

			if (Runner.Stage == default)
				throw new InvalidOperationException();

			FixedUpdateInternal();
		}

		public void ManualRender()
		{
			if (_manualUpdate == false)
				throw new InvalidOperationException("Manual update is not turned on");

			if (Runner.Stage != default)
				throw new InvalidOperationException();

			RenderInternal();
		}

		public void EnableLogging(bool value)
		{
			_enableLogging = value;

			for (int i = 0; i < _stateMachines.Count; i++)
			{
				_stateMachines[i].EnableLogging = value;
			}
		}

		// NetworkBehaviour INTERFACE

		public override int? DynamicWordCount => GetNetworkDataWordCount();

		public override void Spawned()
		{
			for (int i = 0; i < _stateMachines.Count; i++)
			{
				var machine = _stateMachines[i];

				machine.EnableLogging = _enableLogging;
				machine.Reset();
			}

			if (HasStateAuthority == false)
			{
				ReadNetworkData();
			}

			for (int i = 0; i < _stateMachines.Count; i++)
			{
				_stateMachines[i].Initialize(this, Runner);
			}

			if (HasStateAuthority == true)
			{
				WriteNetworkData();
			}
		}

		public override void Render()
		{
			if (_manualUpdate == true)
				return;

			RenderInternal();
		}

		public override void FixedUpdateNetwork()
		{
			if (_manualUpdate == true)
				return;

			FixedUpdateInternal();
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			if (hasState == false)
				return;

			for (int i = 0; i < _stateMachines.Count; i++)
			{
				_stateMachines[i].Deinitialize(hasState);
			}
		}

		// IBeforeAllTicks INTERFACE

		void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount)
		{
			ReadNetworkData();
		}

		// IAfterTick INTERFACE

		void IAfterTick.AfterTick()
		{
			if (Object.IsProxy == true && IsInterpolationDataPredicted() == false)
				return;

			WriteNetworkData();
		}

		// PRIVATE METHODS

		private void FixedUpdateInternal()
		{
			if (Object.IsProxy == true && IsInterpolationDataPredicted() == false)
				return;

			for (int i = 0; i < _stateMachines.Count; i++)
			{
				_stateMachines[i].FixedUpdateNetwork();
			}
		}

		private void RenderInternal()
		{
			if (Interpolate() == false)
				return; // Wait for interpolation data before starting rendering

			for (int i = 0; i < _stateMachines.Count; i++)
			{
				var machine = _stateMachines[i];

			#if UNITY_EDITOR
				machine.EnableLogging = _enableLogging;
			#endif

				machine.Render();
			}
		}

		private void CollectStateMachines()
		{
			_stateMachines.Clear();

			var owners = GetComponentsInChildren<IStateMachineOwner>(true);
			var tempMachines = ListPool.Get<IStateMachine>(32);

			for (int i = 0; i < owners.Length; i++)
			{
				owners[i].CollectStateMachines(tempMachines);

				for (int j = 0; j < tempMachines.Count; j++)
				{
					var stateMachine = tempMachines[j];

					Assert.Check(_stateMachines.Contains(stateMachine) == false, $"Trying to add already collected state machine for second time {stateMachine.Name}");
					CheckDuplicateStates(stateMachine.Name, stateMachine.States);

					_stateMachines.Add(stateMachine);
				}

				tempMachines.Clear();
			}

			_stateMachinesCollected = true;

			ListPool.Return(tempMachines);
		}

		private int GetNetworkDataWordCount()
		{
			if (_stateMachinesCollected == false)
			{
				CollectStateMachines();
			}

			int wordCount = 0;

			for (int i = 0; i < _stateMachines.Count; i++)
			{
				wordCount += _stateMachines[i].WordCount;
			}

			return wordCount;
		}
		
		private unsafe void ReadNetworkData()
		{
			int* ptr = Ptr;

			for (int i = 0; i < _stateMachines.Count; i++)
			{
				var stateMachine = _stateMachines[i];

				stateMachine.Read(ptr);
				ptr += stateMachine.WordCount;
			}
		}

		private unsafe void WriteNetworkData()
		{
			int* ptr = Ptr;

			for (int i = 0; i < _stateMachines.Count; i++)
			{
				var stateMachine = _stateMachines[i];

				stateMachine.Write(ptr);
				ptr += stateMachine.WordCount;
			}
		}

		private unsafe bool Interpolate()
		{
			if (GetInterpolationData(out InterpolationData interpolationData) == false)
				return false;

			for (int i = 0; i < _stateMachines.Count; i++)
			{
				var stateMachine = _stateMachines[i];

				stateMachine.Interpolate(interpolationData);

				interpolationData.From += stateMachine.WordCount;
				interpolationData.To   += stateMachine.WordCount;
			}

			return true;
		}

		// DEBUG

		[Conditional("DEBUG")]
		private void CheckDuplicateStates(string stateMachineName, IState[] states)
		{
			if (_statePool == null)
			{
				_statePool = new List<IState>(128);
				_statePool.AddRange(states);
				return;
			}

			for (int i = 0; i < states.Length; i++)
			{
				var state = states[i];

				if (_statePool.Contains(state) == true)
				{
					throw new InvalidOperationException($"State {state.Name} is used for multiple state machines, this is not allowed! State Machine: {stateMachineName}");
				}
			}

			_statePool.AddRange(states);
		}
	}
}
