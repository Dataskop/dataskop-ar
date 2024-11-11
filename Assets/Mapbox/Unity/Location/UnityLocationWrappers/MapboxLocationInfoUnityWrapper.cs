namespace Mapbox.Unity.Location {

	using UnityEngine;


	/// <summary>
	/// Wrapper to use Unity's LocationInfo as MapboxLocationInfo
	/// </summary>
	public struct MapboxLocationInfoUnityWrapper : IMapboxLocationInfo {

		public MapboxLocationInfoUnityWrapper(LocationInfo locationInfo) {
			_locationInfo = locationInfo;
		}

		private LocationInfo _locationInfo;


		public float latitude => _locationInfo.latitude;

		public float longitude => _locationInfo.longitude;

		public float altitude => _locationInfo.altitude;

		public float horizontalAccuracy => _locationInfo.horizontalAccuracy;

		public float verticalAccuracy => _locationInfo.verticalAccuracy;

		public double timestamp => _locationInfo.timestamp;

	}

}