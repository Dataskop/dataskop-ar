using UnityEngine;

namespace Dataskop.Data {

	public class MeasurementResultsResponse {

		public int Count { get; }

		public MeasurementResultRange MeasurementResults { get; }

		public MeasurementResultsResponse(int count, MeasurementResultRange measurementResults) {
			Count = count;
			MeasurementResults = measurementResults;
		}

	}

}