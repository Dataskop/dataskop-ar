//-----------------------------------------------------------------------
// <copyright file="CanonicalTileIdTest.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;

namespace Mapbox.MapboxSdkCs.UnitTest {

	using Map;
	using Mapbox.Utils;
	using NUnit.Framework;

	[TestFixture]
	internal class CanonicalTileIdTest {

		[Test]
		public void ToVector2d() {
			HashSet<CanonicalTileId> set = TileCover.Get(Vector2dBounds.World(), 5);

			foreach (CanonicalTileId tile in set) {
				UnwrappedTileId reverse = TileCover.CoordinateToTileId(tile.ToVector2d(), 5);

				Assert.AreEqual(tile.Z, reverse.Z);
				Assert.AreEqual(tile.X, reverse.X);
				Assert.AreEqual(tile.Y, reverse.Y);
			}
		}

	}

}