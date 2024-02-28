using UnityEngine;

namespace DataskopAR {

	public static class AccountManager {

#region Constants
		
			private const string APITokenKey = "API_TOKEN";

#endregion
		
#region Properties

		public static bool IsLoggedIn => PlayerPrefs.HasKey(APITokenKey) && !string.IsNullOrEmpty(PlayerPrefs.GetString(APITokenKey));

#endregion

#region Methods

		public static void Login(string loginToken) {
			PlayerPrefs.SetString(APITokenKey, loginToken);
		}

		public static void Logout() {

			if (PlayerPrefs.HasKey(APITokenKey)) {
				PlayerPrefs.DeleteKey(APITokenKey);
			}

			SceneMaster.LoadScene(0);

		}

		public static string GetLoginToken() {
			return IsLoggedIn ? PlayerPrefs.GetString(APITokenKey) : string.Empty;
		}

#endregion

	}

}