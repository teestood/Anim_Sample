using Animancer;
using UnityEngine;

namespace Animations.AnimancerFSM
{
	public class PlayerLocomotionState : PlayerStateBehaviour
	{
		[SerializeField]
		private LinearMixerTransition _moveMixer;

		protected override void OnEnterStateRender()
		{
			Animancer.Play(_moveMixer);

			// Update the animation time based on the state time
			_moveMixer.State.Time = Machine.StateTime;
		}

		protected override void OnRender()
		{
			_moveMixer.State.Parameter = Controller.InterpolatedSpeed;
		}
	}
}
