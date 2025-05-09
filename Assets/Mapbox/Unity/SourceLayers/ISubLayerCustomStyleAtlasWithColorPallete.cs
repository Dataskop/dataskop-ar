﻿namespace Mapbox.Unity.Map {

	using UnityEngine;
	using MeshGeneration.Data;

	public interface ISubLayerCustomStyleAtlasWithColorPallete : ISubLayerCustomStyleOptions, ISubLayerStyle {

		ScriptablePalette ColorPalette { get; set; }

		void SetAsStyle(Material TopMaterial, Material SideMaterial, AtlasInfo uvAtlas, ScriptablePalette palette);

	}

}