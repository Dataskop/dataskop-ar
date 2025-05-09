﻿using System;

namespace Mapbox.Map {

	/// <summary>
	///     Unwrapped tile identifier in a slippy map. Similar to <see cref="CanonicalTileId"/>,
	///     but might go around the globe.
	/// </summary>
	public struct UnwrappedTileId : IEquatable<UnwrappedTileId> {

		/// <summary> The zoom level. </summary>
		public readonly int Z;

		/// <summary> The X coordinate in the tile grid. </summary>
		public readonly int X;

		/// <summary> The Y coordinate in the tile grid. </summary>
		public readonly int Y;

		/// <summary>
		///     Initializes a new instance of the <see cref="UnwrappedTileId"/> struct,
		///     representing a tile coordinate in a slippy map that might go around the
		///     globe.
		/// </summary>
		/// <param name="z">The z coordinate.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public UnwrappedTileId(int z, int x, int y) {
			Z = z;
			X = x;
			Y = y;
		}

		/// <summary> Gets the canonical tile identifier. </summary>
		/// <value> The canonical tile identifier. </value>
		public CanonicalTileId Canonical => new(this);

		/// <summary>
		///     Returns a <see cref="T:System.String"/> that represents the current
		///     <see cref="T:Mapbox.Map.UnwrappedTileId"/>.
		/// </summary>
		/// <returns>
		///     A <see cref="T:System.String"/> that represents the current
		///     <see cref="T:Mapbox.Map.UnwrappedTileId"/>.
		/// </returns>
		public override string ToString() {
			return Z + "/" + X + "/" + Y;
		}

		public bool Equals(UnwrappedTileId other) {
			return X == other.X && Y == other.Y && Z == other.Z;
		}

		public override int GetHashCode() {
			return X << 6 ^ Y << 16 ^ Z << 8;
		}

		public override bool Equals(object obj) {
			return X == ((UnwrappedTileId)obj).X && Y == ((UnwrappedTileId)obj).Y && Z == ((UnwrappedTileId)obj).Z;
		}

		public static bool operator ==(UnwrappedTileId a, UnwrappedTileId b) {
			return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
		}

		public static bool operator !=(UnwrappedTileId a, UnwrappedTileId b) {
			return !(a == b);
		}

		public UnwrappedTileId North => new(Z, X, Y - 1);

		public UnwrappedTileId East => new(Z, X + 1, Y);

		public UnwrappedTileId South => new(Z, X, Y + 1);

		public UnwrappedTileId West => new(Z, X - 1, Y);

		public UnwrappedTileId NorthEast => new(Z, X + 1, Y - 1);

		public UnwrappedTileId SouthEast => new(Z, X + 1, Y + 1);

		public UnwrappedTileId NorthWest => new(Z, X - 1, Y - 1);

		public UnwrappedTileId SouthWest => new(Z, X - 1, Y + 1);

	}

}