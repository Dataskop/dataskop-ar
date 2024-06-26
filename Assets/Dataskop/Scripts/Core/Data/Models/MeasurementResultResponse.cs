using System.Collections.Generic;

namespace Dataskop.Data {

	public class MeasurementResultsResponse {

		public int Count { get; }

		public ICollection<MeasurementResult> MeasurementResults { get; }

		public MeasurementResultsResponse(int count, ICollection<MeasurementResult> measurementResults) {
			Count = count;
			MeasurementResults = measurementResults;
		}

	}

}