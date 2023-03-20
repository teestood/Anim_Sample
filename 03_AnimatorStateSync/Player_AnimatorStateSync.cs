using Fusion;
using UnityEngine;

namespace Animations.AnimatorStateSynchronization
{
	[OrderAfter(typeof(CharacterController))]
	public class Player_AnimatorStateSync : NetworkBehaviour
	{
		// PRIVATE MEMBERS

		private CharacterController _controller;
		private Animator _animator;
		private int _lastVisibleJump;

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			_lastVisibleJump = _controller.JumpCount;
		}

		public override void Render()
		{
			UpdateAnimations();
		}

		// MONOBEHAVIOUR

		protected void Awake()
		{
			_controller = GetComponentInChildren<CharacterController>();
			_animator = GetComponentInChildren<Animator>();
		}

		// PRIVATE METHODS

		private void UpdateAnimations()
		{
			if (_lastVisibleJump < _controller.InterpolatedJumpCount)
			{
				_animator.SetTrigger("Jump");
			}
			else if (_lastVisibleJump > _controller.InterpolatedJumpCount)
			{
				// Cancel Jump
			}

			_lastVisibleJump = _controller.InterpolatedJumpCount;

			_animator.SetFloat("Speed", _controller.InterpolatedSpeed);
		}
	}
}
