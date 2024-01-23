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
		private float BestGPSAccuracy { get; set; } = 1000;

		/// <summary>
		/// Is true, if the initial geo location has been acquired already.
		/// </summary>
		private bool HasInitialLocationData { get; set; }

		private bool HasUsedFixedPositioning { get; set; }

#endregion

#region Methods

		private void OnEnable() {
			locationProvider.DeviceLocationProvider.OnLocationUpdated += UpdateGPSLocation;
			locationProvider.mapManager.OnUpdated += UpdateMapRoot;
		}

		/// <summary>
		/// Grab initial location accuracy
		/// </summary>
		public void InitializeGeoLocation() {

			float gpsAccuracy = locationProvider.DeviceLocationProvider.CurrentLocation.Accuracy;

			if (gpsAccuracy > 20) {
				ErrorHandler.ThrowError(201, gpsAccuracy, this);
			}

			BestGPSAccuracy = gpsAccuracy;
			locationProvider.mapManager.UpdateMap(locationProvider.DeviceLocationProvider.CurrentLocation.LatitudeLongitude);

			locationProvider.DeviceLocationProvider.CurrentLocation.LatitudeLongitude.ToVector3xz();
			HasInitialLocationData = true;

		}

		/// <summary>
		/// Update Map when user location gps over device is more accurate than latest sample
		/// </summary>
		private void UpdateGPSLocation(Location userLocation) {

			if (HasUsedFixedPositioning)
				return;

			if (!HasInitialLocationData)
				return;

			if (userLocation.Accuracy > BestGPSAccuracy)
				return;

			if (Mathf.Abs(userLocation.Accuracy - BestGPSAccuracy) < 0.5f)
				return;

			locationProvider.mapManager.UpdateMap(userLocation.LatitudeLongitude);
			BestGPSAccuracy = userLocation.Accuracy;

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
			BestGPSAccuracy = 0;
			Handheld.Vibrate();
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
			locationProvider.DeviceLocationProvider.OnLocationUpdated -= UpdateGPSLocation;
			locationProvider.mapManager.OnUpdated -= UpdateMapRoot;
		}

#endregion

	}

}