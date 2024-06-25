#nullable enable
using UnityEngine;

namespace Dataskop {

	public static class AccountManager {

		private const string APITokenKey = "API_TOKEN";

		public static bool IsLoggedIn => TryGetLoginToken() != null;

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

	}

}