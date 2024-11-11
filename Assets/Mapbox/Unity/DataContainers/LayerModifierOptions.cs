namespace Mapbox.Unity.Map {

	using System;
	using System.Collections.Generic;
	using MeshGeneration.Modifiers;
	using Utilities;

	[Serializable]
	public class LayerModifierOptions {

		public PositionTargetType moveFeaturePositionTo;
		[NodeEditorElement("Mesh Modifiers")]
		public List<MeshModifier> MeshModifiers;
		[NodeEditorElement("Game Object Modifiers")]
		public List<GameObjectModifier> GoModifiers;

	}

}