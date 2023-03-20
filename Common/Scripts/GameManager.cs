using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

namespace Animations
{
	[RequireComponent(typeof(NetworkRunner))]
	[RequireComponent(typeof(NetworkEvents))]
	public sealed class GameManager : SimulationBehaviour, IPlayerJoined, IPlayerLeft
	{
		// PUBLIC MEMBERS

		public event Action PlayerJoined;

		// PRIVATE MEMBERS

		[SerializeField]
		private NetworkObject _playerPrefab;
		[SerializeField]
		private Vector3 _playerOffset = new(2f, 0f, 0f);
		[SerializeField]
		private Vector3 _playerRotation = new(90f, 0f, 0f);

		private Dictionary<PlayerRef, NetworkObject> _players = new(32);

		// IPlayerJoined INTERFACE

		void IPlayerJoined.PlayerJoined(PlayerRef playerRef)
		{
			if (Runner.IsServer == false)
				return;

			var position = _players.Count * _playerOffset;
			var rotation = Quaternion.Euler(_playerRotation);

			var player = Runner.Spawn(_playerPrefab, position, rotation, inputAuthority: playerRef);

			_players.Add(playerRef, player);

			Runner.SetPlayerObject(playerRef, player);

			PlayerJoined?.Invoke();
		}

		// IPlayerLeft INTERFACE

		void IPlayerLeft.PlayerLeft(PlayerRef playerRef)
		{
			if (Runner.IsServer == false)
				return;

			if (_players.TryGetValue(playerRef, out NetworkObject player) == false)
				return;

			Runner.Despawn(player);
			_players.Remove(playerRef);
		}

		// MONOBEHAVIOUR

		private void Awake()
		{
			var networkEvents = GetComponent<NetworkEvents>();
			networkEvents.OnSceneLoadDone.AddListener(OnSceneLoaded);
		}

		// PRIVATE METHODS

		private void OnSceneLoaded(NetworkRunner runner)
		{
			var behaviours = runner.SimulationUnityScene.FindObjectsOfTypeInOrder<SimulationBehaviour>();

			for (int i = 0; i < behaviours.Length; i++)
			{
				Runner.AddSimulationBehaviour(behaviours[i]);
			}
		}
	}
}
