#nullable enable
using UnityEngine;

namespace DataskopAR {

	public static class AccountManager {

#region Constants

		private const string APITokenKey = "API_TOKEN";

#endregion

#region Properties

		public static bool IsLoggedIn => TryGetLoginToken() != null;

#endregion

#region Methods

		private static bool HasToken() {
			return PlayerPrefs.HasKey(APITokenKey);
		}

		public static void Login(string loginToken) {
			PlayerPrefs.SetString(APITokenKey, loginToken);
		}

		public static void Logout() {

			if (HasToken()) {
				PlayerPrefs.DeleteKey(APITokenKey);
			}

			SceneHandler.LoadScene("MainMenu");
		}

		public static string? TryGetLoginToken() {
			string? token = PlayerPrefs.GetString(APITokenKey, null);
			if (token == null) return null;

			return !string.IsNullOrEmpty(token) ? token : null;
		}

#endregion

	}

}