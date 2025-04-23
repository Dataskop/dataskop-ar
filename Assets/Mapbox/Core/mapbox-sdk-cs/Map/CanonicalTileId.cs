//-----------------------------------------------------------------------
// <copyright file="CanonicalTileId.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.Map {

	using System;
	using Utils;

	/// <summary>
	/// Data type to store  <see href="https://en.wikipedia.org/wiki/Web_Mercator"> Web Mercator</see> tile scheme.
	/// <see href="http://www.maptiler.org/google-maps-coordinates-tile-bounds-projection/"> See tile IDs in action. </see>
	/// </summary>
	public struct CanonicalTileId : IEquatable<CanonicalTileId> {

		/// <summary> The zoom level. </summary>
		public readonly int Z;

		/// <summary> The X coordinate in the tile grid. </summary>
		public readonly int X;

		/// <summary> The Y coordinate in the tile grid. </summary>
		public readonly int Y;

		/// <summary>
		///     Initializes a new instance of the <see cref="CanonicalTileId"/> struct,
		///     representing a tile coordinate in a slippy map.
		/// </summary>
		/// <param name="z"> The z coordinate or the zoom level. </param>
		/// <param name="x"> The x coordinate. </param>
		/// <param name="y"> The y coordinate. </param>
		public CanonicalTileId(int z, int x, int y) {
			Z = z;
			X = x;
			Y = y;
		}

		internal CanonicalTileId(UnwrappedTileId unwrapped) {
			int z = unwrapped.Z;
			int x = unwrapped.X;
			int y = unwrapped.Y;

			int wrap = (x < 0 ? x - (1 << z) + 1 : x) / (1 << z);

			Z = z;
			X = x - wrap * (1 << z);
			Y = y < 0 ? 0 : Math.Min(y, (1 << z) - 1);
		}

		/// <summary>
		///     Get the cordinate at the top left of corner of the tile.
		/// </summary>
		/// <returns> The coordinate. </returns>
		public Vector2d ToVector2d() {
			double n = Math.PI - 2.0 * Math.PI * Y / Math.Pow(2.0, Z);

			double lat = 180.0 / Math.PI * Math.Atan(Math.Sinh(n));
			double lng = X / Math.Pow(2.0, Z) * 360.0 - 180.0;

			// FIXME: Super hack because of rounding issues.
			return new Vector2d(lat - 0.0001, lng + 0.0001);
		}

		/// <summary>
		///     Returns a <see cref="T:System.String"/> that represents the current
		///     <see cref="T:Mapbox.Map.CanonicalTileId"/>.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.String"/> that represents the current
		///     <see cref="T:Mapbox.Map.CanonicalTileId"/>.
		/// </returns>
		public override string ToString() {
			return Z + "/" + X + "/" + Y;
		}

		#region Equality

		public bool Equals(CanonicalTileId other) {
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		public override int GetHashCode() {
			return X ^ Y ^ Z;
		}

		public static bool operator ==(CanonicalTileId a, CanonicalTileId b) {
			return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
		}

		public static bool operator !=(CanonicalTileId a, CanonicalTileId b) {
			return !(a == b);
		}

		public override bool Equals(object obj) {
			if (obj is CanonicalTileId) {
				return Equals((CanonicalTileId)obj);
			}
			else {
				return false;
			}
		}

		#endregion

	}

}