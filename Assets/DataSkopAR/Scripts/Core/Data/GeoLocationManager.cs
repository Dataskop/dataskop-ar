using Mapbox.Unity.Location;
using Mapbox.Unity.Utilities;
using UnityEngine;

namespace DataskopAR.Data {

	public class GeoLocationManager : MonoBehaviour {

#region Fields

		[Header("References")]
		[SerializeField] private LocationProviderFactory locationProvider;
		[SerializeField] private Camera arCam;

#endregion

#region Properties

		/// <summary>
		/// Keeps track of the best GPS accuracy the device received.
		/// </summary>
		private float BestAccuracy { get; set; } = 1000;

		/// <summary>
		/// Is true, if the initial geo location has been acquired already.
		/// </summary>
		private bool HasInitialLocationData { get; set; }

		private bool HasUsedFixedPositioning { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			locationProvider.DefaultLocationProvider.OnLocationUpdated += UpdateLocation;
			locationProvider.mapManager.OnUpdated += UpdateMapRoot;
		}

		/// <summary>
		/// Grab initial location accuracy
		/// </summary>
		public void InitializeGeoLocation() {

			Location initialLocation = locationProvider.DefaultLocationProvider.CurrentLocation;
			float gpsAccuracy = initialLocation.Accuracy;

			if (gpsAccuracy > 20) {
				ErrorHandler.ThrowError(201, gpsAccuracy, this);
			}

			BestAccuracy = gpsAccuracy;
			Debug.Log("Received Initial Location: " + initialLocation.LatitudeLongitude);
			locationProvider.mapManager.UpdateMap(initialLocation.LatitudeLongitude, 18);
			HasInitialLocationData = true;

		}



		/// <summary>
		/// Update Map when user location gps over device is more accurate than latest sample
		/// </summary>
		private void UpdateLocation(Location userLocation) {

			if (HasUsedFixedPositioning)
				return;

			if (!HasInitialLocationData)
				return;

			if (userLocation.Accuracy > BestAccuracy)
				return;

			if (Mathf.Abs(userLocation.Accuracy - BestAccuracy) < 0.5f)
				return;

			Debug.Log("Received Location: " + userLocation.LatitudeLongitude);
			locationProvider.mapManager.UpdateMap(userLocation.LatitudeLongitude);
			BestAccuracy = userLocation.Accuracy;

		}

		/// <summary>
		/// Fetch Position manually
		/// </summary>
		public void FetchPosition() {
			locationProvider.mapManager.UpdateMap(locationProvider.DefaultLocationProvider.CurrentLocation.LatitudeLongitude);
		}

		/// <summary>
		/// Updates map to the position of the found QR code's lat/long.
		/// </summary>
		public void OnQRMarkerTracking(QrResult qrResult) {

			if (!qrResult.Code.Contains('@')) {
				return;
			}

			string[] splitResult = qrResult.Code.Split('@', 2);
			string dataPointLocation = splitResult[1];

			locationProvider.mapManager.UpdateMap(Conversions.StringToLatLon(dataPointLocation));
			BestAccuracy = 0;

#if UNITY_ANDROID
			Handheld.Vibrate();
#endif
			HasUsedFixedPositioning = true;

			NotificationHandler.Add(new Notification {
				Category = NotificationCategory.Check,
				Text = "Location Code scanned!",
				DisplayDuration = NotificationDuration.Short
			});

		}

		/// <summary>
		/// Updates the map root to the user position when acquiring a new GPS position.
		/// </summary>
		private void UpdateMapRoot() {
			Vector3 arPos = arCam.transform.position;
			locationProvider.mapManager.Root.position = new Vector3(arPos.x, locationProvider.mapManager.Root.position.y, arPos.z);
		}

		private void OnDisable() {
			locationProvider.DefaultLocationProvider.OnLocationUpdated -= UpdateLocation;
			locationProvider.mapManager.OnUpdated -= UpdateMapRoot;
		}

#endregion

	}

}