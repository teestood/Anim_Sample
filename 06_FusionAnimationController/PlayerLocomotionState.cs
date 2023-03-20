using UnityEngine;
using Fusion.Animations;

namespace Animations.FusionAnimationController
{
	public class PlayerLocomotionState : BlendTreeState
	{
		private CharacterController _characterController;

		protected override Vector2 GetBlendPosition(bool interpolated)
		{
			return new Vector2(0.0f, interpolated == true ? _characterController.InterpolatedSpeed : _characterController.Speed);
		}

		private void Awake()
		{
			_characterController = GetComponentInParent<CharacterController>();
		}
	}
}
