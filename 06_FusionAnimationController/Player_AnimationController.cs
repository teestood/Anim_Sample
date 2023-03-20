using Fusion.Animations;
using Fusion;

namespace Animations.FusionAnimationController
{
	[OrderAfter(typeof(CharacterController))]
	[OrderBefore(typeof(AnimationController))]
	public class Player_AnimationController : NetworkBehaviour
	{
		// PRIVATE MEMBERS

		private CharacterController _controller;

		private PlayerLocomotionState _locomotionState;
		private PlayerJumpState _jumpState;

		// NetworkBehaviour INTERFACE

		public override void FixedUpdateNetwork()
		{
			if (IsProxy == true)
				return;

			if (_controller.HasJumped == true)
			{
				_jumpState.Activate(0.15f);
			}
			else if (_jumpState.IsPlaying() == false || _jumpState.IsFinished(-0.15f, false) == true)
			{
				_locomotionState.Activate(0.15f);
			}
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			_controller = GetComponentInChildren<CharacterController>();

			_locomotionState = GetComponentInChildren<PlayerLocomotionState>();
			_jumpState = GetComponentInChildren<PlayerJumpState>();
		}
	}
}
