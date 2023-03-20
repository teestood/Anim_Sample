using Fusion;
using UnityEngine;

namespace Animations
{
	public class SceneInput : MonoBehaviour
	{
		// PRIVATE MEMBERS

		private static int _lastSingleInputChange;

		// MONOBEHAVIOUR

		protected void Update()
		{
			// Only one single input change per frame is possible (important for multi-peer multi-input game)
			if (_lastSingleInputChange != Time.frameCount)
			{
				if (Input.GetKeyDown(KeyCode.Keypad0) == true)
				{
					SetActiveRunner(-1);
				}
				else if (Input.GetKeyDown(KeyCode.Keypad1) == true)
				{
					SetActiveRunner(0);
				}
				else if (Input.GetKeyDown(KeyCode.Keypad2) == true)
				{
					SetActiveRunner(1);
				}
				else if (Input.GetKeyDown(KeyCode.Keypad3) == true)
				{
					SetActiveRunner(2);
				}
			}
		}

		// PRIVATE METHODS

		private void SetActiveRunner(int index)
		{
			var enumerator = NetworkRunner.GetInstancesEnumerator();

			enumerator.MoveNext(); // Skip first runner, it is a temporary prefab

			int currentIndex = -1;
			while (enumerator.MoveNext() == true)
			{
				currentIndex++;

				var runner = enumerator.Current;

				runner.IsVisible = index < 0 || currentIndex == index;
				runner.ProvideInput = index < 0 || currentIndex == index;
			}
		}
	}
}
