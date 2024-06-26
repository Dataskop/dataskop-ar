using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace Dataskop.Data {

	[UsedImplicitly]
	public class DataAttribute {

		public string ID { get; }

		public string Label { get; }

		public string Type { get; }

		public string Unit { get; }

		public float Minimum { get; }

		public float Maximum { get; }

		public IReadOnlyCollection<VisualizationOption> VisOptions { get; }

		public DataAttribute(string id, string label, string unit, string attributeType, string min, string max,
			IReadOnlyCollection<VisualizationOption> visualizationOptions) {

			ID = id;
			Label = label;
			Unit = unit;

			if (float.TryParse(min, NumberStyles.Float, CultureInfo.InvariantCulture, out float minVal)) {
				Minimum = minVal;
			}

			if (float.TryParse(max, NumberStyles.Float, CultureInfo.InvariantCulture, out float maxVal)) {
				Maximum = maxVal;
			}

			string[] acceptedTypes = {
				"nominal",
				"binary",
				"ordinal",
				"discrete",
				"continuous"
			};

			if (!acceptedTypes.Contains(attributeType)) {
				throw new ArgumentOutOfRangeException(nameof(attributeType), "Type not supported.");
			}

			Type = attributeType;
			VisOptions = visualizationOptions;

		}

	}

}