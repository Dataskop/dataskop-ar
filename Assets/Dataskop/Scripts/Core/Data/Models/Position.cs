using System.Globalization;

namespace Dataskop.Data {

	public class Position {

#region Constructors

		/// <summary>
		///     Creates and returns a position object.
		/// </summary>
		public Position(double latitude, double longitude, double altitude) {
			Latitude = latitude;
			Longitude = longitude;
			Altitude = altitude;
		}

#endregion

#region Methods

		/// <summary>
		///     Returns a latitude and longitude string.
		/// </summary>
		public string GetLatLong() {
			return
				$"{Latitude.ToString(CultureInfo.InvariantCulture)}, {Longitude.ToString(CultureInfo.InvariantCulture)}";
		}

#endregion

#region Properties

		public double Latitude { get; }

		public double Longitude { get; }

		public double Altitude { get; }

#endregion

	}

}