namespace Mapbox.Unity.MeshGeneration.Interfaces {

	using VectorTile;
	using UnityEngine;
	using Data;
	using System;
	using Map;

	/// <summary>
	/// Layer visualizers contains sytling logic and processes features
	/// </summary>
	public abstract class LayerVisualizerBase : ScriptableObject {

		public abstract bool Active { get; }

		public abstract string Key { get; set; }

		public abstract VectorSubLayerProperties SubLayerProperties { get; set; }

		public abstract void Create(VectorTileLayer layer, UnityTile tile,
			Action<UnityTile, LayerVisualizerBase> callback = null);

		public event EventHandler LayerVisualizerHasChanged;

		public virtual void Initialize() {

		}

		public virtual void InitializeStack() {

		}

		public virtual void SetProperties(VectorSubLayerProperties properties) {

		}

		public virtual void Clear() {

		}

		public void UnregisterTile(UnityTile tile) {
			OnUnregisterTile(tile);
		}

		public virtual void OnUnregisterTile(UnityTile tile) {

		}

		public virtual void UnbindSubLayerEvents() {

		}

		protected virtual void OnUpdateLayerVisualizer(EventArgs e) {
			EventHandler handler = LayerVisualizerHasChanged;

			if (handler != null) {
				handler(this, e);
			}
		}

	}

}