using UnityEngine;

namespace Animations.LegacyPerformance
{
	public class PerformanceTest : MonoBehaviour
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private GameObject _characterPrefab;
		[SerializeField]
		private int _spawnAmount = 20;
		[SerializeField]
		private Vector3 _offset = new Vector3(2f, 0f, 0f);
		[SerializeField]
		private Vector3 _rowOffset = new Vector3(0f, 0f, 2f);
		[SerializeField]
		private Vector3 _characterRotation = new Vector3(0f, 90f, 0f);
		[SerializeField]
		private int _rowItems = 10;

		private AnimationData[] _animationData;
		private bool _useSample;

		private float _animationLength;
		private float _animationTime;

		// MONOBEHAVIOUR

		protected void Start()
		{
			var rotation = Quaternion.Euler(_characterRotation);
			_animationData = new AnimationData[_spawnAmount];

			var basePosition = transform.position;

			for (int i = 0; i < _spawnAmount; i++)
			{
				var position = basePosition + (i % _rowItems) * _offset + (i / _rowItems) * _rowOffset;
				var character = Instantiate(_characterPrefab, position, rotation);

				var animation = character.GetComponentInChildren<Animation>();

				_animationData[i] = new AnimationData
				{
					Animation = animation,
					State = animation[animation.clip.name],
				};
			}

			_animationLength = _animationData[0].State.length;
		}

		protected void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space) == true)
			{
				_useSample = !_useSample;

				for (int i = 0; i < _animationData.Length; i++)
				{
					var data = _animationData[i];

					if (_useSample == true)
					{
						data.State.enabled = false;
						data.State.normalizedTime = 0f;
						data.State.weight = 1f;
					}
					else
					{
						data.State.enabled = true;
						data.State.normalizedTime = 0f;
						data.State.weight = 1f;
						data.State.speed = 1f;
					}
				}

				if (_useSample == true)
				{
					Debug.Log("Switched to animation Sampling");
					_animationTime = 0f;
				}
				else
				{
					Debug.Log("Switched to animation default Play");
				}
			}

			if (_useSample == true)
			{
				_animationTime = (_animationTime + Time.deltaTime) % _animationLength;

				for (int i = 0; i < _animationData.Length; i++)
				{
					var data = _animationData[i];

					data.State.time = _animationTime;

					data.State.enabled = true;
					data.Animation.Sample();
					data.State.enabled = false;
				}
			}
		}

		// DATA STRUCTURES

		private class AnimationData
		{
			public Animation Animation;
			public AnimationState State;
		}
	}
}
