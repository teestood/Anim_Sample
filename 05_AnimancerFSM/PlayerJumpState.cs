using Animancer;
using UnityEngine;

namespace Animations.AnimancerFSM
{
	public class PlayerJumpState : PlayerStateBehaviour
	{
		[SerializeField]
		private ClipTransition _jumpClip;

		protected override void OnEnterStateRender()
		{
			Animancer.Play(_jumpClip);
		}

		protected override void OnFixedUpdate()
		{
			if (Machine.StateTime >= _jumpClip.Length * _jumpClip.Speed)
			{
				// Jump animation should be finished, let's leave this state
				Machine.TryDeactivateState(StateId);
			}
		}
	}
}
