//-----------------------------------------------------------------------
// <copyright file="Vector2dBounds.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.Utils {

	/// <summary> Represents a bounding box derived from a southwest corner and a northeast corner. </summary>
	public struct Vector2dBounds {

		/// <summary> Southwest corner of bounding box. </summary>
		public Vector2d SouthWest;

		/// <summary> Northeast corner of bounding box. </summary>
		public Vector2d NorthEast;

		/// <summary> Initializes a new instance of the <see cref="Vector2dBounds" /> struct. </summary>
		/// <param name="sw"> Geographic coordinate representing southwest corner of bounding box. </param>
		/// <param name="ne"> Geographic coordinate representing northeast corner of bounding box. </param>
		public Vector2dBounds(Vector2d sw, Vector2d ne) {
			SouthWest = sw;
			NorthEast = ne;
		}

		/// <summary> Gets the south latitude. </summary>
		/// <value> The south latitude. </value>
		public double South => SouthWest.x;

		/// <summary> Gets the west longitude. </summary>
		/// <value> The west longitude. </value>
		public double West => SouthWest.y;

		/// <summary> Gets the north latitude. </summary>
		/// <value> The north latitude. </value>
		public double North => NorthEast.x;

		/// <summary> Gets the east longitude. </summary>
		/// <value> The east longitude. </value>
		public double East => NorthEast.y;

		/// <summary>
		///     Gets or sets the central coordinate of the bounding box. When
		///     setting a new center, the bounding box will retain its original size.
		/// </summary>
		/// <value> The central coordinate. </value>
		public Vector2d Center
		{
			get
			{
				double lat = (SouthWest.x + NorthEast.x) / 2;
				double lng = (SouthWest.y + NorthEast.y) / 2;

				return new Vector2d(lat, lng);
			}

			set
			{
				double lat = (NorthEast.x - SouthWest.x) / 2;
				SouthWest.x = value.x - lat;
				NorthEast.x = value.x + lat;

				double lng = (NorthEast.y - SouthWest.y) / 2;
				SouthWest.y = value.y - lng;
				NorthEast.y = value.y + lng;
			}
		}

		/// <summary>
		///     Creates a bound from two arbitrary points. Contrary to the constructor,
		///     this method always creates a non-empty box.
		/// </summary>
		/// <param name="a"> The first point. </param>
		/// <param name="b"> The second point. </param>
		/// <returns> The convex hull. </returns>
		public static Vector2dBounds FromCoordinates(Vector2d a, Vector2d b) {
			Vector2dBounds bounds = new(a, a);
			bounds.Extend(b);

			return bounds;
		}

		/// <summary> A bounding box containing the world. </summary>
		/// <returns> The world bounding box. </returns>
		public static Vector2dBounds World() {
			Vector2d sw = new(-90, -180);
			Vector2d ne = new(90, 180);

			return new Vector2dBounds(sw, ne);
		}

		/// <summary> Extend the bounding box to contain the point. </summary>
		/// <param name="point"> A geographic coordinate. </param>
		public void Extend(Vector2d point) {
			if (point.x < SouthWest.x) {
				SouthWest.x = point.x;
			}

			if (point.x > NorthEast.x) {
				NorthEast.x = point.x;
			}

			if (point.y < SouthWest.y) {
				SouthWest.y = point.y;
			}

			if (point.y > NorthEast.y) {
				NorthEast.y = point.y;
			}
		}

		/// <summary> Extend the bounding box to contain the bounding box. </summary>
		/// <param name="bounds"> A bounding box. </param>
		public void Extend(Vector2dBounds bounds) {
			Extend(bounds.SouthWest);
			Extend(bounds.NorthEast);
		}

		/// <summary> Whenever the geographic bounding box is empty. </summary>
		/// <returns> <c>true</c>, if empty, <c>false</c> otherwise. </returns>
		public bool IsEmpty() {
			return SouthWest.x > NorthEast.x ||
			       SouthWest.y > NorthEast.y;
		}

		/// <summary>
		/// Converts to an array of doubles.
		/// </summary>
		/// <returns>An array of coordinates.</returns>
		public double[] ToArray() {
			double[] array = {
				SouthWest.x, SouthWest.y, NorthEast.x, NorthEast.y
			};

			return array;
		}

		/// <summary> Converts the Bbox to a URL snippet. </summary>
		/// <returns> Returns a string for use in a Mapbox query URL. </returns>
		public override string ToString() {
			return string.Format("{0},{1}", SouthWest.ToString(), NorthEast.ToString());
		}

	}

}