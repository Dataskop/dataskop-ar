using UnityEngine;

namespace Dataskop.Data {

	public struct VisualizationResultDisplayData {

		public MeasurementResult Result { get; set; }

		public MeasurementType Type { get; set; }

		public Sprite AuthorSprite { get; set; }

		public DataAttribute Attribute { get; set; }

	}

}