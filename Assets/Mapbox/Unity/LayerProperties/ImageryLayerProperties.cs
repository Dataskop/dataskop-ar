namespace Mapbox.Unity.Map {

	using UnityEngine;
	using System;
	using System.Collections.Generic;

	[Serializable]
	public class ImageryLayerProperties : LayerProperties {

		public ImagerySourceType sourceType = ImagerySourceType.MapboxStreets;

		public LayerSourceOptions sourceOptions = new() {
			isActive = true,
			layerSource = MapboxDefaultImagery.GetParameters(ImagerySourceType.MapboxStreets)
		};

		public ImageryRasterOptions rasterOptions = new();

		public override bool NeedsForceUpdate() {
			return true;
		}

	}

}