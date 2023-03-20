using System;
using Animancer;
using Fusion.FSM;
using UnityEngine;

namespace Animations.AnimancerFSM
{
	// Player behaviour that should be placed on GameObject in Player hierarchy
	// - inherits from standard NetworkBehaviour so standard networked properties can be used
	public class PlayerStateBehaviour  : StateBehaviour<PlayerStateBehaviour>
	{
		[HideInInspector]
		public CharacterController Controller;
		[HideInInspector]
		public AnimancerComponent Animancer;
	}

	// Plain class to be used as potential sub-states
	// - does not inherit from NetworkBehaviour, create reference for parent PlayerStateBehaviour and store networked properties there
	[Serializable]
	public class PlayerState : State<PlayerState>
	{
		[HideInInspector]
		public PlayerStateBehaviour ParentState;
		[HideInInspector]
		public AnimancerComponent Animancer;
	}

	// FSM machine to operate with PlayerStateBehaviours
	public class PlayerBehaviourMachine : StateMachine<PlayerStateBehaviour>
	{
		public PlayerBehaviourMachine(string name, CharacterController controller, AnimancerComponent animancer, params PlayerStateBehaviour[] states) : base(name, states)
		{
			for (int i = 0; i < states.Length; i++)
			{
				var state = states[i];

				state.Controller = controller;
				state.Animancer = animancer;
			}
		}
	}

	// FSM machine to operate with PlayerStates plain classes, can be used as sub-machine
	public class PlayerStateMachine : StateMachine<PlayerState>
	{
		public PlayerStateMachine(string name, PlayerStateBehaviour parentState, AnimancerComponent animancer, params PlayerState[] states) : base(name, states)
		{
			for (int i = 0; i < states.Length; i++)
			{
				var state = states[i];

				state.ParentState = parentState;
				state.Animancer = animancer;
			}
		}
	}
}
