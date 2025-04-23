using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Dataskop.Data {

	public class TokenValidator : MonoBehaviour {

		private const string URL = "https://backend.dataskop.at/api/company/list";

		[Header("Events")]
		public UnityEvent<string, bool> tokenChecked;

		public void Validate(string token) {
			StartCoroutine(CheckResponseStatus(token, status => { tokenChecked?.Invoke(token, status); }));
		}

		private static IEnumerator CheckResponseStatus(string token, Action<bool> callback) {

			using UnityWebRequest request = UnityWebRequest.Get(URL);

			request.SetRequestHeader("Content-Type", "application/json");
			request.SetRequestHeader("Authorization", token);

			UnityWebRequestAsyncOperation op = request.SendWebRequest();

			while (!op.isDone) yield return null;

			if (request.downloadHandler.error != string.Empty) {

				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Error,
						Text = "Unauthorized Token!",
						DisplayDuration = NotificationDuration.Medium
					}
				);

				yield break;
			}

			if (request.result == UnityWebRequest.Result.ProtocolError) {
				Debug.Log(request.error);
			}

			callback(request.result == UnityWebRequest.Result.Success);

		}

	}

}