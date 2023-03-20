using Fusion;
using UnityEngine;

namespace Animations
{
	public enum EInputButtons
	{
		Jump = 0,
	}

	public struct PlayerInput : INetworkInput
	{
		public Vector2        MoveDirection;
		public NetworkButtons Buttons;

		public bool Jump { get { return Buttons.IsSet(EInputButtons.Jump); } set { Buttons.Set((int)EInputButtons.Jump, value); } }
	}

	public class PlayerInputProvider : SimulationBehaviour, ISpawned, IDespawned, IBeforeUpdate
	{
		// PRIVATE MEMBERS

		private PlayerInput _cachedInput;
		private bool _resetCachedInput;

		// NETWORK INTERFACES

		void ISpawned.Spawned()
		{
			if (Runner.LocalPlayer == Object.InputAuthority)
			{
				var events = Runner.GetComponent<NetworkEvents>();

				events.OnInput.RemoveListener(OnInput);
				events.OnInput.AddListener(OnInput);
			}
		}

		void IDespawned.Despawned(NetworkRunner runner, bool hasState)
		{
			var events = Runner.GetComponent<NetworkEvents>();
			events.OnInput.RemoveListener(OnInput);
		}

		void IBeforeUpdate.BeforeUpdate()
		{
			if (Object == null || Object.HasInputAuthority == false)
				return;

			if (_resetCachedInput == true)
			{
				_resetCachedInput = false;
				_cachedInput = default;
			}

			// Input is tracked only if the runner should provide input (important in multipeer mode)
			if (Runner.ProvideInput == false)
				return;

			ProcessKeyboardInput();
		}

		// PRIVATE METHODS

		private void OnInput(NetworkRunner runner, NetworkInput networkInput)
		{
			// Input is polled for single fixed update, but at this time we don't know how many times in a row OnInput() will be executed.
			// This is the reason for having a reset flag instead of resetting input immediately, otherwise we could lose input for next fixed updates (for example move direction).
			_resetCachedInput = true;

			networkInput.Set(_cachedInput);
		}

		private void ProcessKeyboardInput()
		{
			if (Input.GetKey(KeyCode.Space) == true)
			{
				_cachedInput.Jump = true;
			}

			float horizontal = Input.GetAxis("Horizontal");
			float vertical = Input.GetAxis("Vertical");

			if (horizontal != 0f || vertical != 0f)
			{
				_cachedInput.MoveDirection = new Vector2(horizontal, vertical);
			}
		}
	}
}
