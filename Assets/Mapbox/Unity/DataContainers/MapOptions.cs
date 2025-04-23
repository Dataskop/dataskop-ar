namespace Mapbox.Unity.Map {

	using System;
	using UnityEngine;

	[Serializable]
	public class MapOptions : MapboxDataProperty {

		public MapLocationOptions locationOptions = new();
		public MapExtentOptions extentOptions = new(MapExtentType.RangeAroundCenter);
		public MapPlacementOptions placementOptions = new();
		public MapScalingOptions scalingOptions = new();
		[Tooltip("Texture used while tiles are loading.")]
		public Texture2D loadingTexture = null;
		public Material tileMaterial = null;

	}

	[Serializable]
	public class EditorPreviewOptions : MapboxDataProperty {

		public bool isPreviewEnabled = false;

	}

}