using JetBrains.Annotations;

namespace Dataskop.Data {

	[UsedImplicitly]
	public class VisualizationStyle {

		public bool IsTimeSeries { get; set; }

		public bool HasDropShadow { get; set; }

		public bool HasGroundLine { get; set; }

		public VisualizationStyle(bool timeSeries, bool dropShadow, bool groundLine) {

			IsTimeSeries = timeSeries;
			HasDropShadow = dropShadow;
			HasGroundLine = groundLine;

		}

	}

}