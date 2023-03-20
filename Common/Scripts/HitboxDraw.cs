using UnityEngine;
using Fusion;

namespace Animations
{
	[OrderAfter(typeof(HitboxManager))]
	public class HitboxDraw : SimulationBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private bool _drawHitbox = true;
		[SerializeField]
		private Hitbox _hitbox;
		[SerializeField]
		private int _hitboxDrawIntervalTicks = 50;
		[SerializeField]
		private bool _subtickAccuracy;

		[Header("Colors")]
		[SerializeField]
		private Color _inputAuthorityColor = Color.white;
		[SerializeField]
		private Color _proxyColor = Color.gray;
		[SerializeField]
		private Color _stateAuthorityColor = Color.blue;

		// SimulationBehaviour INTERFACE

		public override void FixedUpdateNetwork()
		{
			if (_drawHitbox == true)
			{
				DrawHitbox();
			}
		}

		// PRIVATE METHODS

		private void DrawHitbox()
		{
			if (_hitbox == null)
				return;

			if (HasInputAuthority == true && HasStateAuthority == false)
				return; // Do not draw for input authority, no point

			if (Runner.IsForward == false)
				return;

			int tick = Runner.Simulation.Tick;

			if (Object.HasStateAuthority == false)
			{
				int fromTick = Runner.Simulation.InterpFrom.Tick;
				int toTick   = Runner.Simulation.InterpTo.Tick;

				if (_subtickAccuracy == true)
				{
					tick = Mathf.RoundToInt(Mathf.Lerp(fromTick, toTick, Runner.Simulation.InterpAlpha));
				}
				else
				{
					tick = Runner.Simulation.InterpAlpha < 0.5f ? fromTick : toTick;
				}
			}

			if (tick % _hitboxDrawIntervalTicks == 0)
			{
				float duration = _hitboxDrawIntervalTicks * Runner.DeltaTime;
				GameDraw.WireBox(_hitbox.Position, _hitbox.transform.rotation, _hitbox.BoxExtents * 2f, GetColor(), duration);
			}
		}

		private Color GetColor()
		{
			if (Object.IsProxy == true)
				return _proxyColor;

			return Object.HasStateAuthority == true ? _stateAuthorityColor : _inputAuthorityColor;
		}
	}
}
