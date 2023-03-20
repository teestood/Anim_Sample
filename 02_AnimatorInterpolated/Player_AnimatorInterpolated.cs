using Fusion;
using UnityEngine;

namespace Animations.AnimatorInterpolated
{
	[OrderAfter(typeof(CharacterController))]
	public class Player_AnimatorInterpolated : NetworkBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private bool _useInterpolation = true;

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
			int jumpCount = _useInterpolation == true ? _controller.InterpolatedJumpCount : _controller.JumpCount;

			if (_lastVisibleJump < jumpCount)
			{
				_animator.SetTrigger("Jump");
			}
			else if (_lastVisibleJump > jumpCount)
			{
				// Cancel Jump
			}

			_lastVisibleJump = jumpCount;

			float speed = _useInterpolation == true ? _controller.InterpolatedSpeed : _controller.Speed;
			_animator.SetFloat("Speed", speed);
		}
	}
}
