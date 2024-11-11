//-----------------------------------------------------------------------
// <copyright file="Utils.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.MapboxSdkCs.UnitTest {

	using System.Collections.Generic;
	using Map;


	internal static class Utils {

		internal class VectorMapObserver : Mapbox.Utils.IObserver<VectorTile> {

			private List<VectorTile> tiles = new();

			public List<VectorTile> Tiles => tiles;

			public void OnNext(VectorTile tile) {
				if (tile.CurrentState == Tile.State.Loaded) {
					tiles.Add(tile);
				}
			}

		}

		internal class RasterMapObserver : Mapbox.Utils.IObserver<RasterTile> {

			private List<byte[]> tiles = new();

			public List<byte[]> Tiles => tiles;

			public void OnNext(RasterTile tile) {
				if (tile.CurrentState == Tile.State.Loaded && !tile.HasError) {
					tiles.Add(tile.Data);
				}
			}

		}

		internal class ClassicRasterMapObserver : Mapbox.Utils.IObserver<ClassicRasterTile> {

			private List<byte[]> tiles = new();

			public List<byte[]> Tiles => tiles;

			public void OnNext(ClassicRasterTile tile) {
				if (tile.CurrentState == Tile.State.Loaded && !tile.HasError) {
					tiles.Add(tile.Data);
				}
			}

		}

	}

}