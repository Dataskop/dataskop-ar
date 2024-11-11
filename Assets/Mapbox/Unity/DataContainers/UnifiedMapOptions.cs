namespace Mapbox.Unity.Map {

	using System;
	using Utilities;

	[Serializable]
	public class UnifiedMapOptions {

		public MapPresetType mapPreset = MapPresetType.LocationBasedMap;
		public MapOptions mapOptions = new();
		[NodeEditorElement("Image Layer")]
		public ImageryLayerProperties imageryLayerProperties = new();
		[NodeEditorElement("Terrain Layer")]
		public ElevationLayerProperties elevationLayerProperties = new();
		[NodeEditorElement("Vector Layer")]
		public VectorLayerProperties vectorLayerProperties = new();

	}

}