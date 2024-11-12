﻿#if UNITY_ANDROID
namespace Mapbox.Unity.Telemetry {

	using UnityEngine;

	public static class AndroidJavaObjectExtensions {

		public static AndroidJavaObject ClassForName(string className) {
			using (AndroidJavaClass clazz = new("java.lang.Class")) {
				return clazz.CallStatic<AndroidJavaObject>("forName", className);
			}
		}

		// Cast extension method
		public static AndroidJavaObject Cast(this AndroidJavaObject source, string destClass) {
			using (AndroidJavaObject destClassAJC = ClassForName(destClass)) {
				return destClassAJC.Call<AndroidJavaObject>("cast", source);
			}
		}

	}

	public class TelemetryAndroid : ITelemetryLibrary {

		private AndroidJavaObject _activityContext = null;
		private AndroidJavaObject _telemInstance = null;

		private static ITelemetryLibrary _instance = new TelemetryAndroid();

		public static ITelemetryLibrary Instance => _instance;

		public void Initialize(string accessToken) {
			if (string.IsNullOrEmpty(accessToken)) {
				throw new System.ArgumentNullException("accessToken");
			}

			using (AndroidJavaClass activityClass = new("com.unity3d.player.UnityPlayer")) {
				_activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
			}

			if (null == _activityContext) {
				Debug.LogError("Could not get current activity");
				return;
			}

			_telemInstance = new AndroidJavaObject(
				"com.mapbox.android.telemetry.MapboxTelemetry",
				_activityContext,
				accessToken,
				"MapboxEventsUnityAndroid/" + Constants.SDK_VERSION
			);

			if (null == _telemInstance) {
				Debug.LogError("Could not get class 'MapboxTelemetry'");
				return;
			}
			else {
				_telemInstance.Call<bool>("disable");
			}
		}

		public void SendTurnstile() {
			using (AndroidJavaObject MapboxAndroidTurnstileEvent = new(
				       "com.mapbox.android.telemetry.AppUserTurnstile", "MapboxEventsUnityAndroid",
				       Constants.SDK_VERSION
			       )) {
				if (null == MapboxAndroidTurnstileEvent) {
					Debug.LogError("Could not get class 'AppUserTurnstile'");
					return;
				}

				MapboxAndroidTurnstileEvent.Call("setSkuId", Constants.SDK_SKU_ID);
				_telemInstance.Call<bool>("push", MapboxAndroidTurnstileEvent);
			}
		}

		public void SetLocationCollectionState(bool enable) {
			if (enable) {
				_telemInstance.Call<bool>("enable");
			}
			else {
				_telemInstance.Call<bool>("disable");
			}
			//_telemInstance.Call(
			//	"setTelemetryEnabled"
			//	, enable
			//);
		}

	}

}
#endif