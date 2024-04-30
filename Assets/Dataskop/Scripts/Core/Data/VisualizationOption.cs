using System;
using System.Linq;
using JetBrains.Annotations;

namespace DataskopAR.Data {

	[UsedImplicitly]
	public class VisualizationOption {

#region Constructor

		public VisualizationOption(string type, VisualizationStyle style) {

			string[] acceptedTypes = {
				"dot",
				"bubble",
				"bar",
				"box"
			};

			if (!acceptedTypes.Contains(type)) {
				throw new ArgumentOutOfRangeException(nameof(type), "Type not supported.");
			}

			Type = type;
			Style = style;

		}

#endregion

#region Properties

		public string Type { get; set; }

		public VisualizationStyle Style { get; set; }

#endregion

	}

}