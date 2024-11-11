using Mapbox.Unity.Map;

namespace Mapbox.Unity.SourceLayers {

	public class SubLayerModeling : ISubLayerModeling {

		private VectorSubLayerProperties _subLayerProperties;

		public SubLayerModeling(VectorSubLayerProperties subLayerProperties) {
			_subLayerProperties = subLayerProperties;
		}

		public ISubLayerCoreOptions CoreOptions => _subLayerProperties.coreOptions;

		public ISubLayerExtrusionOptions ExtrusionOptions => _subLayerProperties.extrusionOptions;

		public ISubLayerColliderOptions ColliderOptions => _subLayerProperties.colliderOptions;

		public ISubLayerLineGeometryOptions LineOptions => _subLayerProperties.lineGeometryOptions;

		/// <summary>
		/// Enable terrain snapping for features which sets vertices to terrain
		/// elevation before extrusion.
		/// </summary>
		/// <param name="isEnabled">Enabled terrain snapping</param>
		public virtual void EnableSnapingTerrain(bool isEnabled) {
			if (_subLayerProperties.coreOptions.snapToTerrain != isEnabled) {
				_subLayerProperties.coreOptions.snapToTerrain = isEnabled;
				_subLayerProperties.coreOptions.HasChanged = true;
			}
		}

		/// <summary>
		/// Enable combining individual features meshes into one to minimize gameobject
		/// count and draw calls.
		/// </summary>
		/// <param name="isEnabled"></param>
		public virtual void EnableCombiningMeshes(bool isEnabled) {
			if (_subLayerProperties.coreOptions.combineMeshes != isEnabled) {
				_subLayerProperties.coreOptions.combineMeshes = isEnabled;
				_subLayerProperties.coreOptions.HasChanged = true;
			}
		}

	}

}