using System;

namespace Dataskop {

	public struct TimeRange {

		public DateTime StartTime { get; }

		public DateTime EndTime { get; }

		public TimeRange(DateTime start, DateTime end) {

			StartTime = start;
			EndTime = end;

			if (EndTime < StartTime) {
				EndTime = start;
				StartTime = end;
			}

		}

	}

}