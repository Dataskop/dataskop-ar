using System;
using System.Collections.Generic;

namespace Dataskop {

	public static class TimeRangeUtils {

		public static TimeRange[] GetTimeRangeGaps(TimeRange searchRange, TimeRange[] availableRanges) {

			List<TimeRange> missingTimeRanges = new();
			DateTime previousEndTime = searchRange.StartTime;

			for (int i = availableRanges.Length - 1; i >= 0; i--) {
				TimeRange availableTimeRange = availableRanges[i];

				if (searchRange.StartTime > availableTimeRange.EndTime) {
					continue;
				}

				if (searchRange.EndTime >= availableTimeRange.StartTime && searchRange.EndTime <= availableTimeRange.EndTime) {

					if (searchRange.EndTime > previousEndTime) {
						missingTimeRanges.Add(new TimeRange(previousEndTime, availableTimeRange.StartTime));
					}

					break;
				}

				if (searchRange.EndTime <= availableTimeRange.StartTime) {
					missingTimeRanges.Add(new TimeRange(previousEndTime, searchRange.EndTime));
				}
				else {
					missingTimeRanges.Add(new TimeRange(previousEndTime, availableTimeRange.StartTime));
					previousEndTime = availableTimeRange.EndTime;
				}

			}

			return missingTimeRanges.ToArray();

		}

	}

}