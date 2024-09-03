using System;

namespace Dataskop {

	public readonly struct TimeRange {

		public DateTime StartTime { get; }

		public DateTime EndTime { get; }

		public TimeSpan Span => EndTime - StartTime;

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