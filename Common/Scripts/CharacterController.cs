using Fusion;
using UnityEngine;

namespace Animations
{
	// CharacterController is a dummy controller that just saves
	// speed and jump data based on the processed input
	public class CharacterController : NetworkBehaviour
	{
		// PUBLIC MEMBERS

		[Networked, HideInInspector]
		public int     JumpCount              { get; set; }
		[Networked, HideInInspector]
		public float   Speed                  { get; set;}

		public bool    HasJumped              { get; private set; }

		public int     InterpolatedJumpCount  => _jumpCountInterpolator.Value;
		public float   InterpolatedSpeed      => _speedInterpolator.Value;

		// PRIVATE MEMBERS

		[Networked]
		private NetworkButtons _lastButtonsInput { get; set; }

		private Interpolator<int> _jumpCountInterpolator;
		private Interpolator<float> _speedInterpolator;

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			_jumpCountInterpolator = GetInterpolator<int>(nameof(JumpCount));
			_speedInterpolator = GetInterpolator<float>(nameof(Speed));
		}

		public override void FixedUpdateNetwork()
		{
			if (Object.IsProxy == true)
				return;

			var input = GetInput<PlayerInput>();

			if (input.HasValue == true)
			{
				ProcessInput(input.Value);
			}
		}

		// PRIVATE METHODS

		private void ProcessInput(PlayerInput input)
		{
			HasJumped = input.Buttons.WasPressed(_lastButtonsInput, EInputButtons.Jump);

			if (HasJumped == true)
			{
				JumpCount++;
			}

			// In reality speed can probably be just taken from already existing
			// networked data (e.g. from KCC addon - kcc.Data.RealSpeed) so it
			// won't be necessary to store it anywhere else.
			Speed = input.MoveDirection.magnitude;

			_lastButtonsInput = input.Buttons;
		}
	}
}
