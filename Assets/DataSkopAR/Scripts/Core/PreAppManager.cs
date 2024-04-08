using UnityEngine;

namespace DataskopAR {

	public class PreAppManager : MonoBehaviour {

#region Methods

		private void Start() {

			SetApplicationTargetFrameRate(60);
			if (SceneHandler.GetCurrentScene() == 0) {
				SceneHandler.LoadScene(AccountManager.IsLoggedIn ? 2 : 1);
			}

		}

		private static void SetApplicationTargetFrameRate(int targetFrameRate) {
			Application.targetFrameRate = targetFrameRate;
		}

#endregion

	}

}