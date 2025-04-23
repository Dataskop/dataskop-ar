namespace Mapbox.Unity.MeshGeneration.Factories {

	using Data;
	using System;

	public class TileProcessFinishedEventArgs : EventArgs {

		public AbstractTileFactory Factory;
		public UnityTile Tile;

		public TileProcessFinishedEventArgs(AbstractTileFactory vectorTileFactory, UnityTile tile) {
			Factory = vectorTileFactory;
			Tile = tile;
		}

	}

}