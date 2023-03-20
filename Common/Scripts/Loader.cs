using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion.Animations
{
	public class Loader : MonoBehaviour
	{
		// Called directly from UI
		public void LoadScene(int index)
		{
			SceneManager.LoadScene(index);
		}
	}
}
