using UnityEngine;

namespace DataSkopAR {

	public static class AccountManager {

#region Properties

		public static bool IsLoggedIn => PlayerPrefs.HasKey("API_TOKEN") && !string.IsNullOrEmpty(PlayerPrefs.GetString("API_TOKEN"));

#endregion

#region Methods

		public static void Login(string loginToken) {
			PlayerPrefs.SetString("API_TOKEN", loginToken);
		}

		public static void Logout() {

			if (PlayerPrefs.HasKey("API_TOKEN")) {
				PlayerPrefs.DeleteKey("API_TOKEN");
			}

			SceneMaster.LoadScene(0);

		}

		public static string GetLoginToken() {
			return IsLoggedIn ? PlayerPrefs.GetString("API_TOKEN") : string.Empty;
		}

#endregion

	}

}