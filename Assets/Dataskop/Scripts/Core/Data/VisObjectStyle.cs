using System;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	[Serializable]
	public struct VisObjectStyle {

		public Material defaultMaterial;
		public Material hoverMaterial;
		public Material selectionMaterial;
		public Material timeMaterial;
		public Color primaryColor;
		public Color secondaryColor;

	}

}