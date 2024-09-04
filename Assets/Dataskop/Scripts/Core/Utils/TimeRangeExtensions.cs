using System;
using System.Collections.Generic;

namespace Dataskop {

	public static class TimeRangeExtensions {

		public static TimeRange[] GetTimeRangeGaps(TimeRange searchRange, TimeRange[] availableRanges) {

			List<TimeRange> missingTimeRanges = new();

			DateTime previousEndTime = searchRange.StartTime;

			for (int i = availableRanges.Length - 1; i >= 0; i--) {

				TimeRange availableTimeRange = availableRanges[i];

				if (IsInTimeRange(searchRange.StartTime, availableTimeRange) && IsInTimeRange(searchRange.EndTime, availableTimeRange)) {
					break;
				}

				if (searchRange.StartTime > availableTimeRange.EndTime) {
					continue;
				}

				if (IsInTimeRange(searchRange.EndTime, availableTimeRange)) {
					missingTimeRanges.Add(new TimeRange(previousEndTime, availableTimeRange.StartTime));
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

		public static bool IsInTimeRange(DateTime dateToCheck, TimeRange range) {
			return dateToCheck >= range.StartTime && dateToCheck <= range.EndTime;
		}

	}

}