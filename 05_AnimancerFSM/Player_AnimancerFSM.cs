using System.Collections.Generic;
using Animancer;
using Fusion;
using Fusion.FSM;

namespace Animations.AnimancerFSM
{
	[OrderAfter(typeof(CharacterController))]
	[OrderBefore(typeof(StateMachineController))]
	public class Player_AnimancerFSM : NetworkBehaviour, IStateMachineOwner
	{
		// PRIVATE MEMBERS

		private CharacterController _controller;
		private PlayerBehaviourMachine _fullBodyMachine;

		// NetworkBehaviour INTERFACE

		public override void FixedUpdateNetwork()
		{
			if (IsProxy == true)
				return;

			if (_controller.HasJumped == true)
			{
				_fullBodyMachine.TryActivateState<PlayerJumpState>();
			}
		}

		// IStateMachineOwner INTERFACE

		void IStateMachineOwner.CollectStateMachines(List<IStateMachine> stateMachines)
		{
			var states = GetComponentsInChildren<PlayerStateBehaviour>();
			var animancer = GetComponentInChildren<AnimancerComponent>();

			_fullBodyMachine = new PlayerBehaviourMachine("Full Body", _controller, animancer, states);
			stateMachines.Add(_fullBodyMachine);
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			_controller = GetComponentInChildren<CharacterController>();
		}
	}
}
