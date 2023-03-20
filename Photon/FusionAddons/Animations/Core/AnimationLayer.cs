namespace Fusion.Animations
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Animations;
	using UnityEngine.Playables;

	/// <summary>
	/// Animation layer component.
	/// </summary>
	public unsafe partial class AnimationLayer : MonoBehaviour, IAnimationStateOwner, IAnimationWeightProvider, IAnimationFadingProvider
	{
		// PUBLIC MEMBERS

		/// <summary>
		/// Reference to animation controller.
		/// </summary>
		public AnimationController Controller => _controller;

		/// <summary>
		/// List of animation states (direct children).
		/// </summary>
		public IList<AnimationState> States => _states;

		/// <summary>
		/// Playable mixer which represents this layer and mixes outputs from states.
		/// </summary>
		public AnimationMixerPlayable Mixer => _mixer;

		/// <summary>
		/// Input port used to identify connection to owner (controller) mixer.
		/// </summary>
		public int Port => _port;

		/// <summary>
		/// Weight of the layer.
		/// </summary>
		public float Weight { get { return _weight; } protected set { _weight = value; } }

		/// <summary>
		/// Fading speed of the layer. Controls calculation of <c>Weight</c> over time.
		/// </summary>
		public float FadingSpeed { get { return _fadingSpeed; } protected set { _fadingSpeed = value; } }

		/// <summary>
		/// Interpolated weight used in render update to get smooth animations.
		/// </summary>
		public float InterpolatedWeight { get { return _interpolatedWeight; } protected set { _interpolatedWeight = value; } }

		// PRIVATE MEMBERS

		[SerializeField]
		private AvatarMask             _avatarMask;
		[SerializeField]
		private bool                   _isAdditive;
		[SerializeField]
		public float                   _initialWeight = 1.0f;

		private AnimationController    _controller;
		private AnimationState[]       _states;
		private AnimationMixerPlayable _mixer;
		private string                 _type;
		private int                    _port;
		private float                  _weight;
		private float                  _fadingSpeed;
		private float                  _cachedWeight;
		private float                  _interpolatedWeight;

		// PUBLIC METHODS

		/// <summary>
		/// Returns <c>true</c> if the layer is fading in or if the <c>Weight</c> is greater than zero and the layer is not fading out.
		/// </summary>
		public bool IsActive()
		{
			return (_fadingSpeed == 0.0f && _weight > 0.0f) || _fadingSpeed > 0.0f;
		}

		/// <summary>
		/// Returns <c>true</c> if the layer is fading in or if the <c>Weight</c> is greater than zero.
		/// </summary>
		public bool IsPlaying()
		{
			return _fadingSpeed > 0.0f || _weight > 0.0f;
		}

		/// <summary>
		/// Returns <c>true</c> if the layer is fading in (fading speed greater than zero).
		/// </summary>
		public bool IsFadingIn()
		{
			return _fadingSpeed > 0.0f;
		}

		/// <summary>
		/// Returns <c>true</c> if the layer is fading out (fading speed lower than zero).
		/// </summary>
		public bool IsFadingOut()
		{
			return _fadingSpeed < 0.0f;
		}

		/// <summary>
		/// Activate the layer.
		/// <param name="fadeDuration">How long it takes to reach full <c>Weight</c> from zero.</param>
		/// </summary>
		public void Activate(float fadeDuration)
		{
			if ((_fadingSpeed == 0.0f && _weight >= 1.0f) || _fadingSpeed > 0.0f)
				return;

			if (fadeDuration <= 0.0f)
			{
				_weight      = 1.0f;
				_fadingSpeed = 0.0f;
			}
			else
			{
				_fadingSpeed = 1.0f / fadeDuration;
			}

			Controller.Log($"{nameof(AnimationLayer)}.{nameof(Activate)} ({name}), Fade Duration: {fadeDuration:F3}", gameObject);

			OnActivate();
		}

		/// <summary>
		/// Deactivate the layer.
		/// <param name="fadeDuration">How long it takes to reach zero <c>Weight</c> from full.</param>
		/// </summary>
		public void Deactivate(float fadeDuration)
		{
			if ((_fadingSpeed == 0.0f && _weight <= 0.0f) || _fadingSpeed < 0.0f)
				return;

			if (fadeDuration <= 0.0f)
			{
				_weight      = 0.0f;
				_fadingSpeed = 0.0f;
			}
			else
			{
				_fadingSpeed = 1.0f / -fadeDuration;
			}

			Controller.Log($"{nameof(AnimationLayer)}.{nameof(Deactivate)} ({name}), Fade Duration: {fadeDuration:F3}", gameObject);

			OnDeactivate();
		}

		/// <summary>
		/// Returns <c>true</c> if the layer has an active state. This call is non-recursive.
		/// </summary>
		public bool HasActiveState()
		{
			AnimationState[] states = _states;
			for (int i = 0, count = states.Length; i < count; ++i)
			{
				AnimationState state = states[i];
				if (state.IsActive(true) == true)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns <c>true</c> if the layer has an active state of type <c>T</c>. This call is non-recursive.
		/// </summary>
		public bool HasActiveState<T>() where T : class
		{
			AnimationState[] states = _states;
			for (int i = 0, count = states.Length; i < count; ++i)
			{
				AnimationState state = states[i];
				if (state.IsActive(true) == true && state is T stateAsT)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns active state. This call is non-recursive.
		/// </summary>
		public AnimationState GetActiveState()
		{
			AnimationState[] states = _states;
			for (int i = 0, count = states.Length; i < count; ++i)
			{
				AnimationState state = states[i];
				if (state.IsActive(true) == true)
					return state;
			}

			return null;
		}

		/// <summary>
		/// Returns active state of type T. This call is non-recursive.
		/// </summary>
		public bool GetActiveState<T>(out T activeState) where T : class
		{
			AnimationState[] states = _states;
			for (int i = 0, count = states.Length; i < count; ++i)
			{
				AnimationState state = states[i];
				if (state.IsActive(true) == true && state is T stateAsT)
				{
					activeState = stateAsT;
					return true;
				}
			}

			activeState = default;
			return false;
		}

		/// <summary>
		/// Deactivate all states within this layer.
		/// <param name="fadeDuration">How long it takes to reach zero <c>Weight</c> from full.</param>
		/// <param name="recursive">If true all sub-states will be deactivated as well.</param>
		/// </summary>
		public void DeactivateAllStates(float fadeDuration, bool recursive)
		{
			AnimationState[] states = _states;
			for (int i = 0, count = states.Length; i < count; ++i)
			{
				AnimationState state = states[i];

				if (recursive == true)
				{
					state.DeactivateAllStates(fadeDuration, true);
				}

				state.Deactivate(fadeDuration, true);
			}
		}

		/// <summary>
		/// Called when Animation Controller is initialized.
		/// </summary>
		public void Initialize(AnimationController controller)
		{
			_controller = controller;
			_type       = GetType().Name;

			InitializeStates();

			OnInitialize();
		}

		/// <summary>
		/// Called when Animation Controller is deinitialized.
		/// </summary>
		public void Deinitialize()
		{
			OnDeinitialize();

			DeinitializeStates();

			_controller = default;
		}

		/// <summary>
		/// Called when Animation Controller is spawned.
		/// </summary>
		public void Spawned()
		{
			_mixer              = AnimationMixerPlayable.Create(_controller.Graph);
			_port               = _controller.Mixer.AddInput(_mixer, 0, _initialWeight);
			_weight             = _initialWeight;
			_cachedWeight       = _initialWeight;
			_fadingSpeed        = 0.0f;
			_interpolatedWeight = _initialWeight;

			_controller.Mixer.SetLayerAdditive((uint)_port, _isAdditive);

			if (_avatarMask != null)
			{
				_controller.Mixer.SetLayerMaskFromAvatarMask((uint)_port, _avatarMask);
			}

			AnimationState[] states = _states;
			for (int i = 0, count = states.Length; i < count; ++i)
			{
				AnimationState state = states[i];
				state.Spawned();
			}

			OnSpawned();
		}

		/// <summary>
		/// Called when Animation Controller is despawned.
		/// </summary>
		public void Despawned()
		{
			OnDespawned();

			AnimationState[] states = _states;
			for (int i = 0, count = states != null ? states.Length : 0; i < count; ++i)
			{
				AnimationState state = states[i];
				if (state != null)
				{
					state.Despawned();
				}
			}

			if (_mixer.IsValid() == true)
			{
				_mixer.Destroy();
			}
		}

		/// <summary>
		/// Manual fixed update execution, called internally by Animation Controller.
		/// </summary>
		public void ManualFixedUpdate()
		{
			if (_fadingSpeed <= 0.0f && _weight <= 0.0f)
				return;

			AnimationProfiler.BeginDetailedSample(_type);

			AnimationState[] states = _states;
			for (int i = 0, count = states.Length; i < count; ++i)
			{
				AnimationState state = states[i];
				state.ManualFixedUpdate();
			}

			if (_fadingSpeed != 0.0f)
			{
				_weight += _fadingSpeed * _controller.DeltaTime;

				if (_weight <= 0.0f)
				{
					_weight      = 0.0f;
					_fadingSpeed = 0.0f;
				}
				else if (_weight >= 1.0f)
				{
					_weight      = 1.0f;
					_fadingSpeed = 0.0f;
				}
			}

			OnFixedUpdate();

			AnimationProfiler.EndDetailedSample();
		}

		/// <summary>
		/// Manual interpolation, called internally by Animation Controller.
		/// </summary>
		public void Interpolate()
		{
			if (_interpolatedWeight <= 0.0f)
				return;

			AnimationProfiler.BeginDetailedSample(_type);

			AnimationState[] states = _states;
			for (int i = 0, count = states.Length; i < count; ++i)
			{
				AnimationState state = states[i];
				state.Interpolate();
			}

			OnInterpolate();

			AnimationProfiler.EndDetailedSample();
		}

		/// <summary>
		/// Returns input weight used in playable graph. Used internally by Animation Controller.
		/// </summary>
		public float GetPlayableWeight()
		{
			return _controller.Mixer.GetInputWeight(_port);
		}

		/// <summary>
		/// Calculates and applies weights in playable graph. Requires normalization, final weights can be different than weights stores in layers/states.
		/// <param name="interpolated">If <c>true</c> interpolated properties will be used.</param>
		/// </summary>
		public void SetPlayableWeights(bool interpolated)
		{
			float layerWeight = interpolated == true ? _interpolatedWeight : _weight;
			if (layerWeight <= 0.0f)
			{
				SetPlayableWeight(0.0f);
				return;
			}

			AnimationState[] states     = _states;
			int              stateCount = states.Length;

			if (stateCount == 0)
			{
				SetPlayableWeight(layerWeight);
				return;
			}

			if (stateCount == 1)
			{
				AnimationState state = states[0];
				float playableWeight = state.CalculatePlayableWeights(interpolated, out float maxChildWeight);
				state.SetPlayableWeight(playableWeight > 0.0f ? 1.0f : 0.0f);
				SetPlayableWeight(maxChildWeight * layerWeight);
				return;
			}

			float maxWeight      = 0.0f;
			float childrenWeight = 0.0f;

			for (int i = 0; i < stateCount; ++i)
			{
				childrenWeight += states[i].CalculatePlayableWeights(interpolated, out float maxChildWeight);

				if (maxChildWeight > maxWeight)
				{
					maxWeight = maxChildWeight;
				}
			}

			if (childrenWeight == 1.0f || childrenWeight == 0.0f)
			{
				for (int i = 0; i < stateCount; ++i)
				{
					states[i].ApplyPlayableWeight();
				}
			}
			else
			{
				float weightMultiplier = 1.0f / childrenWeight;

				for (int i = 0; i < stateCount; ++i)
				{
					states[i].ApplyPlayableWeight(weightMultiplier);
				}

				if (childrenWeight > 1.0f)
				{
					childrenWeight = 1.0f;
				}
			}

			if (childrenWeight > maxWeight)
			{
				maxWeight = childrenWeight;
			}

			SetPlayableWeight(maxWeight * layerWeight);
		}

		/// <summary>
		/// Returns first state of type <c>T</c>.
		/// <param name="recursive">Search through children states, using Depth First Search.</param>
		/// </summary>
		public T FindState<T>(bool recursive = true) where T : class
		{
			return FindState<T>(out T state, recursive) == true ? state : default;
		}

		/// <summary>
		/// Returns first state of type <c>T</c>.
		/// <param name="recursive">Search through children states, using Depth First Search.</param>
		/// </summary>
		public bool FindState<T>(out T state, bool recursive = true) where T : class
		{
			AnimationState[] states = _states;
			for (int i = 0, count = states.Length; i < count; ++i)
			{
				AnimationState layerState = states[i];
				if (layerState is T layerStateAsT)
				{
					state = layerStateAsT;
					return true;
				}

				if (recursive == true && layerState.FindState<T>(out T innerState, true) == true)
				{
					state = innerState;
					return true;
				}
			}

			state = default;
			return false;
		}

		public virtual void OnInspectorGUI()
		{
#if UNITY_EDITOR
			//UnityEditor.EditorGUILayout.LabelField("", .ToString("0.00"));
#endif
		}

		// AnimationLayer INTERFACE

		protected virtual void OnInitialize()   {}
		protected virtual void OnDeinitialize() {}
		protected virtual void OnSpawned()      {}
		protected virtual void OnDespawned()    {}
		protected virtual void OnFixedUpdate()  {}
		protected virtual void OnInterpolate()  {}
		protected virtual void OnActivate()     {}
		protected virtual void OnDeactivate()   {}

		// IAnimationStateOwner INTERFACE

		AnimationMixerPlayable IAnimationStateOwner.Mixer => _mixer;

		bool IAnimationStateOwner.IsActive(bool self)
		{
			return (_fadingSpeed == 0.0f && _weight > 0.0f) || _fadingSpeed > 0.0f;
		}

		bool IAnimationStateOwner.IsPlaying(bool self)
		{
			return _fadingSpeed > 0.0f || _weight > 0.0f;
		}

		bool IAnimationStateOwner.IsFadingIn(bool self)
		{
			return _fadingSpeed > 0.0f;
		}

		bool IAnimationStateOwner.IsFadingOut(bool self)
		{
			return _fadingSpeed < 0.0f;
		}

		void IAnimationStateOwner.Activate(AnimationState source, float fadeDuration)
		{
			IList<AnimationState> states = States;
			for (int i = 0, count = states.Count; i < count; ++i)
			{
				AnimationState state = states[i];
				if (state.Port != source.Port)
				{
					state.Deactivate(fadeDuration, true);
				}
			}
		}

		void IAnimationStateOwner.Deactivate(AnimationState source, float fadeDuration)
		{
		}

		// IAnimationWeightProvider INTERFACE

		float IAnimationWeightProvider.Weight             { get { return _weight;             } set { _weight             = value; } }
		float IAnimationWeightProvider.InterpolatedWeight { get { return _interpolatedWeight; } set { _interpolatedWeight = value; } }

		// IAnimationFadingProvider INTERFACE

		float IAnimationFadingProvider.FadingSpeed { get { return _fadingSpeed; } set { _fadingSpeed = value; } }

		// PRIVATE METHODS

		private void SetPlayableWeight(float weight)
		{
			if (weight == _cachedWeight)
				return;

			_cachedWeight = weight;
			_controller.Mixer.SetInputWeight(_port, weight);
		}

		private void InitializeStates()
		{
			if (_states != null)
				return;

			List<AnimationState> activeStates = new List<AnimationState>(8);

			Transform root = transform;
			for (int i = 0, count = root.childCount; i < count; ++i)
			{
				Transform child = root.GetChild(i);

				AnimationState state = child.GetComponentNoAlloc<AnimationState>();
				if (state != null && state.enabled == true && state.gameObject.activeSelf == true)
				{
					activeStates.Add(state);
				}
			}

			_states = activeStates.ToArray();

			AnimationState[] states = _states;
			for (int i = 0, count = states != null ? states.Length : 0; i < count; ++i)
			{
				AnimationState state = states[i];
				state.Initialize(_controller, this);
			}
		}

		private void DeinitializeStates()
		{
			AnimationState[] states = _states;
			for (int i = 0, count = states != null ? states.Length : 0; i < count; ++i)
			{
				AnimationState state = states[i];
				if (state != null)
				{
					state.Deinitialize();
				}
			}

			_states = null;
		}
	}
}
