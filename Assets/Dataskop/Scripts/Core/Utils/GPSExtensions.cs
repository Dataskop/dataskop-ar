using System;
using System.Collections.Generic;
using Mapbox.Utils;

namespace Dataskop.Utils {

	// https://stackoverflow.com/a/7199522
	public static class GPSExtensions {

		private const double Pi = 3.14159265;
		private const double TwoPi = Pi * 2;

		public static bool IsCoordinateInPolygon(Vector2d userLocation, List<Vector2d> latLonPoints) {

			int i;
			int n = latLonPoints.Count;
			double angle = 0;

			for (i = 0; i < n; i++) {
				Vector2d point1 = latLonPoints[i] - userLocation;
				Vector2d point2 = latLonPoints[(i + 1) % n] - userLocation;
				angle += Angle2D(point1.x, point1.y, point2.x, point2.y);
			}

			return !(Math.Abs(angle) < Pi);

		}

		private static double Angle2D(double y1, double x1, double y2, double x2) {

			double theta1 = Math.Atan2(y1, x1);
			double theta2 = Math.Atan2(y2, x2);
			double deltaTheta = theta2 - theta1;

			while (deltaTheta > Pi)
				deltaTheta -= TwoPi;

			while (deltaTheta < -Pi)
				deltaTheta += TwoPi;

			return deltaTheta;

		}

		public static IEnumerable<Vector2d> GenerateRandomLocationsNear(Vector2d location, int amount, double radius) {

			Vector2d[] randomLocations = new Vector2d[amount];
			Random r = new();

			for (int i = 0; i < randomLocations.Length; i++) {

				double latInRange =
					RandomExtensions.NextDoubleInRange(r, location.x - radius, location.x + radius);

				double longInRange =
					RandomExtensions.NextDoubleInRange(r, location.y - radius, location.y + radius);

				randomLocations[i] = new Vector2d(latInRange, longInRange);
			}

			return randomLocations;
		}

	}

}