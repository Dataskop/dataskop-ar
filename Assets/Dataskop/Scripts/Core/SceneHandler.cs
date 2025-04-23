using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;

namespace Dataskop {

	/// <summary>
	/// Manages Scenes and their functions across the app.
	/// </summary>
	public static class SceneHandler {

		/// <summary>
		/// Loads and switches to a scene.
		/// </summary>
		/// <param name="sceneId">The scene ID</param>
		public static void LoadScene(int sceneId) {
			LoaderUtility.Deinitialize();
			LoaderUtility.Initialize();
			SceneManager.LoadScene(sceneId);
		}

		/// <summary>
		/// Loads and switches to a scene.
		/// </summary>
		/// <param name="sceneName">The scene name</param>
		public static void LoadScene(string sceneName) {
			SceneManager.LoadScene(sceneName);
		}

		/// <summary>
		/// Returns the currently active scene.
		/// </summary>
		/// <returns>Int Index in the build menu</returns>
		public static int GetCurrentScene() {
			return SceneManager.GetActiveScene().buildIndex;
		}

	}

}