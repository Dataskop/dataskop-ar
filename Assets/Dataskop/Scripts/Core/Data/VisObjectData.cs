using Dataskop.Data;
using UnityEngine;

namespace Dataskop.Entities.Visualizations {

	public struct VisObjectData {

		public MeasurementResult Result { get; set; }

		public MeasurementType Type { get; set; }

		public DataAttribute Attribute { get; set; }

		public Sprite AuthorSprite { get; set; }

		public Color32 Color { get; set; }

	}

}