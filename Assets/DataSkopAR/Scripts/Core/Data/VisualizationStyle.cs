using JetBrains.Annotations;

namespace DataskopAR.Data {

	[UsedImplicitly]
	public class VisualizationStyle {

#region Properties

		public bool IsTimeSeries { get; set; }

		public bool HasDropShadow { get; set; }

		public bool HasGroundLine { get; set; }

#endregion

#region Constructor

		public VisualizationStyle(bool timeSeries, bool dropShadow, bool groundLine) {

			IsTimeSeries = timeSeries;
			HasDropShadow = dropShadow;
			HasGroundLine = groundLine;

		}

#endregion

	}

}