using UnityEngine;

namespace Dataskop {

	public class PreAppManager : MonoBehaviour {

#region Methods

		private void Awake() {

			FPSManager.SetApplicationTargetFrameRate(30);

			if (SceneHandler.GetCurrentScene() == 0) {
				SceneHandler.LoadScene(AccountManager.IsLoggedIn ? 2 : 1);
			}

		}

#endregion

	}

}