namespace Mapbox.Unity.Location {

	/// <summary>
	/// Wrapper to mock our 'Location' objects as Unity's 'LocationInfo'
	/// </summary>
	public struct MapboxLocationInfoMock : IMapboxLocationInfo {

		public MapboxLocationInfoMock(Location location) {
			_location = location;
		}


		private Location _location;

		public float latitude => (float)_location.LatitudeLongitude.x;

		public float longitude => (float)_location.LatitudeLongitude.y;

		public float altitude => 0f;

		public float horizontalAccuracy => _location.Accuracy;

		public float verticalAccuracy => 0;

		public double timestamp => _location.Timestamp;

	}

}