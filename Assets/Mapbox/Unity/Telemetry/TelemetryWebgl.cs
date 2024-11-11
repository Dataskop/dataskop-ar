namespace Mapbox.Unity.Telemetry {

	using System.Collections.Generic;
	using System.Collections;
	using Json;
	using System;
	using Utilities;
	using UnityEngine;
	using System.Text;
	using UnityEngine.Networking;

	public class TelemetryWebgl : ITelemetryLibrary {

		private string _url;

		private static ITelemetryLibrary _instance = new TelemetryFallback();

		public static ITelemetryLibrary Instance => _instance;

		public void Initialize(string accessToken) {
			_url = string.Format("{0}events/v2?access_token={1}", Utils.Constants.EventsAPI, accessToken);
		}

		public void SendTurnstile() {
			long ticks = DateTime.Now.Ticks;

			if (ShouldPostTurnstile(ticks)) {
				Runnable.Run(PostWWW(_url, GetPostBody()));
				PlayerPrefs.SetString(Constants.Path.TELEMETRY_TURNSTILE_LAST_TICKS_FALLBACK_KEY, ticks.ToString());
			}
		}

		private string GetPostBody() {
			List<Dictionary<string, object>> eventList = new();
			Dictionary<string, object> jsonDict = new();

			long unixTimestamp = (long)Utils.UnixTimestampUtils.To(DateTime.UtcNow);

			jsonDict.Add("event", "appUserTurnstile");
			jsonDict.Add("created", unixTimestamp);
			jsonDict.Add("userId", SystemInfo.deviceUniqueIdentifier);
			jsonDict.Add("enabled.telemetry", false);
			jsonDict.Add("sdkIdentifier", GetSDKIdentifier());
			jsonDict.Add("skuId", Constants.SDK_SKU_ID);
			jsonDict.Add("sdkVersion", Constants.SDK_VERSION);

			// user-agent cannot be set from web broswer, so we send in payload, instead!
			jsonDict.Add("userAgent", GetUserAgent());

			eventList.Add(jsonDict);

			string jsonString = JsonConvert.SerializeObject(eventList);
			return jsonString;
		}

		private bool ShouldPostTurnstile(long ticks) {
			DateTime date = new DateTime(ticks);
			string longAgo = DateTime.Now.AddDays(-100).Ticks.ToString();
			string lastDateString = PlayerPrefs.GetString(
				Constants.Path.TELEMETRY_TURNSTILE_LAST_TICKS_FALLBACK_KEY, longAgo
			);
			long lastTicks = 0;
			long.TryParse(lastDateString, out lastTicks);
			DateTime lastDate = new DateTime(lastTicks);
			TimeSpan timeSpan = date - lastDate;
			return timeSpan.Days >= 1;
		}

		private IEnumerator PostWWW(string url, string bodyJsonString) {
			byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
#if UNITY_2017_1_OR_NEWER
			UnityWebRequest postRequest = new(url, "POST");
			postRequest.SetRequestHeader("Content-Type", "application/json");

			postRequest.downloadHandler = new DownloadHandlerBuffer();
			postRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);

			yield return postRequest.SendWebRequest();
#else
			var headers = new Dictionary<string, string>();
			headers.Add("Content-Type", "application/json");
			headers.Add("user-agent", GetUserAgent());
			var www = new WWW(url, bodyRaw, headers);
			yield return www;
#endif
		}

		private static string GetUserAgent() {
			string userAgent = string.Format(
				"{0}/{1}/{2} MapboxEventsUnity{3}/{4}",
				Application.identifier,
				Application.version,
				"0",
				Application.platform,
				Constants.SDK_VERSION
			);
			return userAgent;
		}

		private string GetSDKIdentifier() {
			string sdkIdentifier = string.Format(
				"MapboxEventsUnity{0}",
				Application.platform
			);
			return sdkIdentifier;
		}

		public void SetLocationCollectionState(bool enable) {
			// empty.
		}

	}

}