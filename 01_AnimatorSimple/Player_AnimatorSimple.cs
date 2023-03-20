using Fusion;
using UnityEngine;

namespace Animations.AnimatorSimple
{
	[OrderAfter(typeof(CharacterController))]
	public class Player_AnimatorSimple : NetworkBehaviour
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
			if (_lastVisibleJump < _controller.JumpCount)
			{
				_animator.SetTrigger("Jump");
			}
			else if (_lastVisibleJump > _controller.JumpCount)
			{
				// Cancel Jump
			}

			_lastVisibleJump = _controller.JumpCount;

			_animator.SetFloat("Speed", _controller.Speed);
		}
	}
}
