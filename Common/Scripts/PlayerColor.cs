using Fusion;
using UnityEngine;

namespace Animations
{
	public class PlayerColor : SimulationBehaviour, ISpawned
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private Color _inputAuthorityColor = Color.white;
		[SerializeField]
		private Color _proxyColor = Color.gray;
		[SerializeField]
		private Color _stateAuthorityColor = Color.blue;

		// ISpawned INTERFACE

		void ISpawned.Spawned()
		{
			var renderer = GetComponentInChildren<SkinnedMeshRenderer>();
			renderer.material.color = GetColor();
		}

		// PRIVATE METHODS

		private Color GetColor()
		{
			if (IsProxy == true)
				return _proxyColor;

			return HasInputAuthority == true ? _inputAuthorityColor : _stateAuthorityColor;
		}
	}
}
