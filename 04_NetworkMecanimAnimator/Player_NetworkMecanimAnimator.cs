using Fusion;

namespace Animations.AnimatorNetworkMecanimAnimator
{
	[OrderAfter(typeof(CharacterController))]
	public class Player_NetworkMecanimAnimator : NetworkBehaviour
	{
		// PRIVATE MEMBERS

		private CharacterController _controller;
		private NetworkMecanimAnimator _networkAnimator;

		// NetworkBehaviour INTERFACE

		public override void FixedUpdateNetwork()
		{
			if (IsProxy == true)
				return;

			if (Runner.IsForward == false)
				return;

			if (_controller.HasJumped == true)
			{
				_networkAnimator.SetTrigger("Jump", true);
			}

			_networkAnimator.Animator.SetFloat("Speed", _controller.Speed);
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			_controller = GetComponentInChildren<CharacterController>();
			_networkAnimator = GetComponentInChildren<NetworkMecanimAnimator>();
		}
	}
}
