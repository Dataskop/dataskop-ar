//-----------------------------------------------------------------------
// <copyright file="Map.cs" company="Mapbox">
//     Copyright (c) 2016 Mapbox. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Mapbox.Map {

	using System;
	using System.Collections.Generic;
	using Platform;
	using Utils;

	/// <summary>
	///     The Mapbox Map abstraction will take care of fetching and decoding
	///     data for a geographic bounding box at a certain zoom level.
	/// </summary>
	/// <typeparam name="T">
	///     The tile type, currently <see cref="T:Mapbox.Map.Vector"/> or
	///     <see cref="T:Mapbox.Map.Raster"/>.
	/// </typeparam>
	/// <example>
	/// Request a map of the whole world:
	/// <code>
	/// var map = new Map&lt;RasterTile&gt;(MapboxAccess.Instance);
	/// map.Zoom = 2
	/// map.Vector2dBounds = Vector2dBounds.World();
	/// map.TilesetId = "mapbox://styles/mapbox/streets-v10
	///
	/// // Register for tile updates.
	/// map.Subscribe(this);
	///
	/// // Trigger the request.
	/// map.Update();
	/// </code>
	/// </example>
	public sealed class Map<T> : Utils.IObservable<T> where T : Tile, new() {

		/// <summary>
		///     Arbitrary limit of tiles this class will handle simultaneously.
		/// </summary>
		public const int TileMax = 256;

		private readonly IFileSource fs;
		private Vector2dBounds latLngBounds;
		private int zoom;
		private string tilesetId;

		private HashSet<T> tiles = new();
		private List<Utils.IObserver<T>> observers = new();

		/// <summary>
		///     Initializes a new instance of the <see cref="T:Mapbox.Map.Map`1"/> class.
		/// </summary>
		/// <param name="fs"> The data source abstraction. </param>
		public Map(IFileSource fs) {
			this.fs = fs;
			latLngBounds = new Vector2dBounds();
			zoom = 0;
		}

		/// <summary>
		///     Gets or sets the tileset ID. If not set, it will use the default
		///     tileset ID for the tile type. I.e. "mapbox.satellite" for raster tiles
		///     and "mapbox.mapbox-streets-v7" for vector tiles.
		/// </summary>
		/// <value>
		///     The tileset ID, usually in the format "user.mapid". Exceptionally,
		///     <see cref="T:Mapbox.Map.RasterTile"/> will take the full style URL
		///     from where the tile is composited from, like "mapbox://styles/mapbox/streets-v9".
		/// </value>
		public string TilesetId
		{
			get => tilesetId;

			set
			{
				if (tilesetId == value) {
					return;
				}

				tilesetId = value;

				foreach (Tile tile in tiles) {
					tile.Cancel();
				}

				tiles.Clear();
			}
		}

		/// <summary>
		///     Gets the tiles, vector or raster. Tiles might be
		///     in a incomplete state.
		/// </summary>
		/// <value> The tiles. </value>
		public HashSet<T> Tiles => tiles;

		/// <summary>Gets or sets a geographic bounding box.</summary>
		/// <value>New geographic bounding box.</value>
		public Vector2dBounds Vector2dBounds
		{
			get => latLngBounds;

			set => latLngBounds = value;
		}

		/// <summary>Gets or sets the central coordinate of the map.</summary>
		/// <value>The central coordinate.</value>
		public Vector2d Center
		{
			get => latLngBounds.Center;

			set => latLngBounds.Center = value;
		}

		/// <summary>Gets or sets the map zoom level.</summary>
		/// <value>The new zoom level.</value>
		public int Zoom
		{
			get => zoom;

			set => zoom = Math.Max(0, Math.Min(20, value));
		}

		/// <summary>
		///     Sets the coordinates bounds and zoom at once.
		/// </summary>
		/// <param name="bounds"> Coordinates bounds. </param>
		/// <param name="zoom"> Zoom level. </param>
		public void SetVector2dBoundsZoom(Vector2dBounds bounds, int zoom) {
			latLngBounds = bounds;
			this.zoom = zoom;
		}

		/// <summary> Add an <see cref="T:IObserver" /> to the observer list. </summary>
		/// <param name="observer"> The object subscribing to events. </param>
		public void Subscribe(Utils.IObserver<T> observer) {
			observers.Add(observer);
		}

		/// <summary> Remove an <see cref="T:IObserver" /> to the observer list. </summary>
		/// <param name="observer"> The object unsubscribing to events. </param>
		public void Unsubscribe(Utils.IObserver<T> observer) {
			observers.Remove(observer);
		}

		private void NotifyNext(T next) {
			List<Utils.IObserver<T>> copy = new(observers);

			foreach (Utils.IObserver<T> observer in copy) {
				observer.OnNext(next);
			}
		}

		/// <summary>
		/// Request tiles after changing map properties.
		/// </summary>
		public void Update() {
			HashSet<CanonicalTileId> cover = TileCover.Get(latLngBounds, zoom);

			if (cover.Count > TileMax) {
				return;
			}

			// Do not request tiles that we are already requesting
			// but at the same time exclude the ones we don't need
			// anymore, cancelling the network request.
			tiles.RemoveWhere(
				(T tile) =>
				{
					if (cover.Remove(tile.Id)) {
						return false;
					}
					else {
						tile.Cancel();
						NotifyNext(tile);

						return true;
					}
				}
			);

			foreach (CanonicalTileId id in cover) {
				T tile = new();

				Tile.Parameters param;
				param.Id = id;
				param.TilesetId = tilesetId;
				param.Fs = fs;

				tile.Initialize(param, () => { NotifyNext(tile); });

				tiles.Add(tile);
			}
		}

	}

}