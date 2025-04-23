using System.Globalization;

namespace Dataskop.Data {

	public class Position {

		public double Latitude { get; }

		public double Longitude { get; }

		public double Altitude { get; }

		/// <summary>
		/// Creates and returns a position object.
		/// </summary>
		public Position(double latitude, double longitude, double altitude) {
			Latitude = latitude;
			Longitude = longitude;
			Altitude = altitude;
		}

		/// <summary>
		/// Returns a latitude and longitude string.
		/// </summary>
		public string GetLatLong() {
			return
				$"{Latitude.ToString(CultureInfo.InvariantCulture)}, {Longitude.ToString(CultureInfo.InvariantCulture)}";
		}

	}

}