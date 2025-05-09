using Mapbox.Unity.Map.Interfaces;

namespace Mapbox.Unity.Map {

	using System.Linq;
	using System.Collections.Generic;
	using UnityEngine;
	using Mapbox.Map;
	using MeshGeneration.Factories;
	using MeshGeneration.Data;
	using System;
	using Platform;
	using UnityEngine.Serialization;
	using Utilities;
	using MeshGeneration.Enums;
	using Mapbox.Unity.MeshGeneration.Interfaces;

	/// <summary>
	/// Map Visualizer
	/// Represents a map.Doesn’t contain much logic and at the moment, it creates requested tiles and relays them to the factories
	/// under itself.It has a caching mechanism to reuse tiles and does the tile positioning in unity world.
	/// Later we’ll most likely keep track of map features here as well to allow devs to query for features easier
	/// (i.e.query all buildings x meters around any restaurant etc).
	/// </summary>
	public abstract class AbstractMapVisualizer : ScriptableObject {

		[SerializeField]
		[NodeEditorElementAttribute("Factories")]
		[FormerlySerializedAs("_factories")]
		public List<AbstractTileFactory> Factories;

		protected IMapReadable _map;
		protected Dictionary<UnwrappedTileId, UnityTile> _activeTiles = new();
		protected Queue<UnityTile> _inactiveTiles = new();
		private int _counter;

		private ModuleState _state;

		public ModuleState State
		{
			get => _state;
			internal set
			{
				if (_state != value) {
					_state = value;
					OnMapVisualizerStateChanged(_state);
				}
			}
		}

		public IMapReadable Map => _map;

		public Dictionary<UnwrappedTileId, UnityTile> ActiveTiles => _activeTiles;

		public Dictionary<UnwrappedTileId, int> _tileProgress;

		public event Action<ModuleState> OnMapVisualizerStateChanged = delegate { };

		public event Action<UnityTile> OnTileFinished = delegate { };

		/// <summary>
		/// Gets the unity tile from unwrapped tile identifier.
		/// </summary>
		/// <returns>The unity tile from unwrapped tile identifier.</returns>
		/// <param name="tileId">Tile identifier.</param>
		public UnityTile GetUnityTileFromUnwrappedTileId(UnwrappedTileId tileId) {
			return _activeTiles[tileId];
		}

		/// <summary>
		/// Initializes the factories by passing the file source down, which is necessary for data (web/file) calls
		/// </summary>
		/// <param name="fileSource"></param>
		public virtual void Initialize(IMapReadable map, IFileSource fileSource) {
			_map = map;
			_tileProgress = new Dictionary<UnwrappedTileId, int>();

			// Allow for map re-use by recycling any active tiles.
			List<UnwrappedTileId> activeTiles = _activeTiles.Keys.ToList();

			foreach (UnwrappedTileId tile in activeTiles) {
				DisposeTile(tile);
			}

			State = ModuleState.Initialized;

			foreach (AbstractTileFactory factory in Factories) {
				if (null == factory) {
					Debug.LogError("AbstractMapVisualizer: Factory is NULL");
				}
				else {
					factory.Initialize(fileSource);
					UnregisterEvents(factory);
					RegisterEvents(factory);
				}
			}
		}

		private void RegisterEvents(AbstractTileFactory factory) {
			factory.OnTileError += Factory_OnTileError;
		}

		private void UnregisterEvents(AbstractTileFactory factory) {
			factory.OnTileError -= Factory_OnTileError;
		}

		public virtual void Destroy() {
			if (Factories != null) {
				_counter = Factories.Count;

				for (int i = 0; i < _counter; i++) {
					if (Factories[i] != null) {
						UnregisterEvents(Factories[i]);
					}
				}
			}

			// Inform all downstream nodes that we no longer need to process these tiles.
			// This scriptable object may be re-used, but it's gameobjects are likely
			// to be destroyed by a scene change, for example.
			foreach (UnwrappedTileId tileId in _activeTiles.Keys.ToList()) {
				DisposeTile(tileId);
			}

			_activeTiles.Clear();
			_inactiveTiles.Clear();
		}

		#region Factory event callbacks

		//factory event callback, not relaying this up for now

		private void TileHeightStateChanged(UnityTile tile) {
			if (tile.HeightDataState == TilePropertyState.Loaded) {
				OnTileHeightProcessingFinished(tile);
			}

			TileStateChanged(tile);
		}

		private void TileRasterStateChanged(UnityTile tile) {
			if (tile.RasterDataState == TilePropertyState.Loaded) {
				OnTileImageProcessingFinished(tile);
			}

			TileStateChanged(tile);
		}

		private void TileVectorStateChanged(UnityTile tile) {
			if (tile.VectorDataState == TilePropertyState.Loaded) {
				OnTileVectorProcessingFinished(tile);
			}

			TileStateChanged(tile);
		}

		public virtual void TileStateChanged(UnityTile tile) {
			bool rasterDone = tile.RasterDataState == TilePropertyState.None ||
			                  tile.RasterDataState == TilePropertyState.Loaded ||
			                  tile.RasterDataState == TilePropertyState.Error ||
			                  tile.RasterDataState == TilePropertyState.Cancelled;

			bool terrainDone = tile.HeightDataState == TilePropertyState.None ||
			                   tile.HeightDataState == TilePropertyState.Loaded ||
			                   tile.HeightDataState == TilePropertyState.Error ||
			                   tile.HeightDataState == TilePropertyState.Cancelled;

			bool vectorDone = tile.VectorDataState == TilePropertyState.None ||
			                  tile.VectorDataState == TilePropertyState.Loaded ||
			                  tile.VectorDataState == TilePropertyState.Error ||
			                  tile.VectorDataState == TilePropertyState.Cancelled;

			if (rasterDone && terrainDone && vectorDone) {
				tile.TileState = TilePropertyState.Loaded;
				OnTileFinished(tile);

				// Check if all tiles in extent are active tiles
				if (_map.CurrentExtent.Count == _activeTiles.Count) {
					bool allDone = true;

					// Check if all tiles are loaded.
					foreach (UnwrappedTileId currentTile in _map.CurrentExtent) {
						allDone = allDone && _activeTiles.ContainsKey(currentTile) &&
						          _activeTiles[currentTile].TileState == TilePropertyState.Loaded;
					}

					if (allDone) {
						State = ModuleState.Finished;
					}
					else {
						State = ModuleState.Working;
					}
				}
				else {
					State = ModuleState.Working;
				}

			}
		}

		#endregion

		/// <summary>
		/// Registers requested tiles to the factories
		/// </summary>
		/// <param name="tileId"></param>
		public virtual UnityTile LoadTile(UnwrappedTileId tileId) {
			UnityTile unityTile = null;

			if (_inactiveTiles.Count > 0) {
				unityTile = _inactiveTiles.Dequeue();
			}

			if (unityTile == null) {
				unityTile = new GameObject().AddComponent<UnityTile>();

				try {
					unityTile.MeshRenderer.sharedMaterial = Instantiate(_map.TileMaterial);
				}
				catch {
					Debug.Log("Tile Material not set. Using default material");
					unityTile.MeshRenderer.sharedMaterial = Instantiate(new Material(Shader.Find("Diffuse")));
				}

				unityTile.transform.SetParent(_map.Root, false);
			}

			unityTile.Initialize(_map, tileId, _map.WorldRelativeScale, _map.AbsoluteZoom, _map.LoadingTexture);
			PlaceTile(tileId, unityTile, _map);

			// Don't spend resources naming objects, as you shouldn't find objects by name anyway!
#if UNITY_EDITOR
			unityTile.gameObject.name = unityTile.CanonicalTileId.ToString();
#endif
			unityTile.OnHeightDataChanged += TileHeightStateChanged;
			unityTile.OnRasterDataChanged += TileRasterStateChanged;
			unityTile.OnVectorDataChanged += TileVectorStateChanged;

			unityTile.TileState = TilePropertyState.Loading;
			ActiveTiles.Add(tileId, unityTile);

			foreach (AbstractTileFactory factory in Factories) {
				factory.Register(unityTile);
			}

			return unityTile;
		}

		public virtual void DisposeTile(UnwrappedTileId tileId) {
			UnityTile unityTile = ActiveTiles[tileId];

			foreach (AbstractTileFactory factory in Factories) {
				factory.Unregister(unityTile);
			}

			unityTile.Recycle();
			ActiveTiles.Remove(tileId);
			_inactiveTiles.Enqueue(unityTile);
		}

		/// <summary>
		/// Repositions active tiles instead of recreating them. Useful for panning the map
		/// </summary>
		/// <param name="tileId"></param>
		public virtual void RepositionTile(UnwrappedTileId tileId) {
			UnityTile currentTile;

			if (ActiveTiles.TryGetValue(tileId, out currentTile)) {
				PlaceTile(tileId, currentTile, _map);
			}
		}

		protected abstract void PlaceTile(UnwrappedTileId tileId, UnityTile tile, IMapReadable map);

		public void ClearMap() {
			UnregisterAllTiles();

			if (Factories != null) {
				foreach (AbstractTileFactory tileFactory in Factories) {
					if (tileFactory != null) {
						tileFactory.Clear();
						DestroyImmediate(tileFactory);
					}
				}
			}

			foreach (UnwrappedTileId tileId in _activeTiles.Keys.ToList()) {
				_activeTiles[tileId].ClearAssets();
				DisposeTile(tileId);
			}

			foreach (UnityTile tile in _inactiveTiles) {
				tile.ClearAssets();
				DestroyImmediate(tile.gameObject);
			}

			_inactiveTiles.Clear();
			State = ModuleState.Initialized;
		}

		public void ReregisterAllTiles() {
			foreach (KeyValuePair<UnwrappedTileId, UnityTile> activeTile in _activeTiles) {
				foreach (AbstractTileFactory abstractTileFactory in Factories) {
					abstractTileFactory.Register(activeTile.Value);
				}
			}
		}

		public void UnregisterAllTiles() {
			foreach (KeyValuePair<UnwrappedTileId, UnityTile> activeTile in _activeTiles) {
				foreach (AbstractTileFactory abstractTileFactory in Factories) {
					abstractTileFactory.Unregister(activeTile.Value);
				}
			}
		}

		public void UnregisterTilesFrom(AbstractTileFactory factory) {
			foreach (KeyValuePair<UnwrappedTileId, UnityTile> tileBundle in _activeTiles) {
				factory.Unregister(tileBundle.Value);
			}
		}

		public void UnregisterAndRedrawTilesFromLayer(VectorTileFactory factory, LayerVisualizerBase layerVisualizer) {
			foreach (KeyValuePair<UnwrappedTileId, UnityTile> tileBundle in _activeTiles) {
				factory.UnregisterLayer(tileBundle.Value, layerVisualizer);
			}

			layerVisualizer.Clear();
			layerVisualizer.UnbindSubLayerEvents();
			layerVisualizer.SetProperties(layerVisualizer.SubLayerProperties);
			layerVisualizer.InitializeStack();

			foreach (KeyValuePair<UnwrappedTileId, UnityTile> tileBundle in _activeTiles) {
				factory.RedrawSubLayer(tileBundle.Value, layerVisualizer);
			}
		}

		public void RemoveTilesFromLayer(VectorTileFactory factory, LayerVisualizerBase layerVisualizer) {
			foreach (KeyValuePair<UnwrappedTileId, UnityTile> tileBundle in _activeTiles) {
				factory.UnregisterLayer(tileBundle.Value, layerVisualizer);
			}

			factory.RemoveVectorLayerVisualizer(layerVisualizer);
		}

		public void ReregisterTilesTo(VectorTileFactory factory) {
			foreach (KeyValuePair<UnwrappedTileId, UnityTile> tileBundle in _activeTiles) {
				factory.Register(tileBundle.Value);
			}
		}

		public void UpdateTileForProperty(AbstractTileFactory factory, LayerUpdateArgs updateArgs) {
			foreach (KeyValuePair<UnwrappedTileId, UnityTile> tileBundle in _activeTiles) {
				factory.UpdateTileProperty(tileBundle.Value, updateArgs);
			}
		}

		#region Events

		/// <summary>
		/// The  <c>OnTileError</c> event triggers when there's a <c>Tile</c> error.
		/// Returns a <see cref="T:Mapbox.Map.TileErrorEventArgs"/> instance as a parameter, for the tile on which error occurred.
		/// </summary>
		public event EventHandler<TileErrorEventArgs> OnTileError;

		private void Factory_OnTileError(object sender, TileErrorEventArgs e) {
			EventHandler<TileErrorEventArgs> handler = OnTileError;

			if (handler != null) {
				handler(this, e);
			}
		}

		/// <summary>
		/// Event delegate, gets called when terrain factory finishes processing a tile.
		/// </summary>
		public event Action<UnityTile> OnTileHeightProcessingFinished = delegate { };

		/// <summary>
		/// Event delegate, gets called when image factory finishes processing a tile.
		/// </summary>
		public event Action<UnityTile> OnTileImageProcessingFinished = delegate { };

		/// <summary>
		/// Event delegate, gets called when vector factory finishes processing a tile.
		/// </summary>
		public event Action<UnityTile> OnTileVectorProcessingFinished = delegate { };

		#endregion

	}

}