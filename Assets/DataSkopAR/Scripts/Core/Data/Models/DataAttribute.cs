using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
namespace DataskopAR.Data {

	[UsedImplicitly]
	public class DataAttribute {

#region Constructors

		public DataAttribute(string id, string label, string unit, string attributeType, string min, string max,
			ICollection<VisualizationOption> visualizationOptions) {

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

#endregion

#region Properties

		public string ID { get; set; }

		public string Label { get; set; }

		public string Type { get; set; }

		public string Unit { get; set; }

		public float Minimum { get; set; }

		public float Maximum { get; set; }

		public ICollection<VisualizationOption> VisOptions { get; set; }

#endregion

	}

}