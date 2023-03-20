using UnityEngine;

namespace Animations.AnimatorSync
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

		// MONOBEHAVIOUR

		protected void Start()
		{
			var rotation = Quaternion.Euler(_characterRotation);
			var basePosition = transform.position;

			for (int i = 0; i < _spawnAmount; i++)
			{
				var position = basePosition + (i % _rowItems) * _offset + (i / _rowItems) * _rowOffset;
				Instantiate(_characterPrefab, position, rotation);
			}
		}
	}
}
