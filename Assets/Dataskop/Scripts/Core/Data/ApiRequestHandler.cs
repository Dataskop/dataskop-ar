using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Dataskop.Data {

	public sealed class ApiRequestHandler {

#region Properties

		public static ApiRequestHandler Instance { get; } = new();

#endregion

#region Methods

		/// <summary>
		///     Performs a GET request to a given API endpoint.
		/// </summary>
		public async Task<string> Get(string url) {

			if (Application.internetReachability == NetworkReachability.NotReachable) {
				HandleClientOffline();
				return "";
			}

			using UnityWebRequest request = UnityWebRequest.Get(url);
			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Authorization", UserData.Instance.Token!);
			UnityWebRequestAsyncOperation operation = request.SendWebRequest();

			while (!operation.isDone) {
				await Task.Yield();
			}

			if (request.result != UnityWebRequest.Result.Success) {
				HandleWebRequestErrors(request);
				return "This did not work!";
			}

			return request.downloadHandler.text;
		}

		/// <summary>
		///     Handles the case when the client is offline.
		/// </summary>
		private void HandleClientOffline() {

			NotificationHandler.Add(new Notification {
				Category = NotificationCategory.Error,
				Text = "You are offline. Make sure you are connected to the internet and try again.",
				DisplayDuration = NotificationDuration.Medium
			});

		}

		/// <summary>
		///     Handles UnityWebRequest errors.
		/// </summary>
		private void HandleWebRequestErrors(UnityWebRequest request) {

			if (request.result == UnityWebRequest.Result.ConnectionError) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = "No connection to the server.",
					DisplayDuration = NotificationDuration.Medium
				});

				return;
			}

			if (request.result == UnityWebRequest.Result.ProtocolError) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = request.responseCode == 401 ? "Your access token is invalid." : "No data available.",
					DisplayDuration = NotificationDuration.Medium
				});

				return;
			}

			NotificationHandler.Add(new Notification {
				Category = NotificationCategory.Error,
				Text = "An unexpected error occurred.",
				DisplayDuration = NotificationDuration.Medium
			});

		}

#endregion

	}

}