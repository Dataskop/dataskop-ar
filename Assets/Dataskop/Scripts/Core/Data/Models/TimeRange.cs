using System;

namespace Dataskop {

	public readonly struct TimeRange {

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

		public bool IsInRange(DateTime dateToCheck) {
			return dateToCheck > StartTime && dateToCheck < EndTime;
		}

	}

}