using System.Collections.Generic;

namespace Dataskop.Data {

	public class MeasurementResultsResponse {

		public int Count { get; }

		public IReadOnlyCollection<MeasurementResult> MeasurementResults { get; }

		public MeasurementResultsResponse(int count, IReadOnlyCollection<MeasurementResult> measurementResults) {
			Count = count;
			MeasurementResults = measurementResults;
		}

	}

}