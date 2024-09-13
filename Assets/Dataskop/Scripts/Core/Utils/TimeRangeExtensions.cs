using System;
using System.Collections.Generic;
using System.Linq;

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
					TimeRange newTimeRange = new(previousEndTime, availableTimeRange.StartTime);
					if (newTimeRange.EndTime - newTimeRange.StartTime > TimeSpan.FromSeconds(1)) {
						missingTimeRanges.Add(newTimeRange);
					}
					break;
				}

				if (searchRange.StartTime >= availableTimeRange.StartTime) {
					previousEndTime = availableTimeRange.EndTime;
					continue;
				}

				if (searchRange.EndTime <= availableTimeRange.StartTime) {
					TimeRange newTimeRange = new(previousEndTime, searchRange.EndTime);
					if (newTimeRange.EndTime - newTimeRange.StartTime > TimeSpan.FromSeconds(1)) {
						missingTimeRanges.Add(newTimeRange);
					}
				}
				else {
					TimeRange newTimeRange = new(previousEndTime, availableTimeRange.StartTime);
					if (newTimeRange.EndTime - newTimeRange.StartTime > TimeSpan.FromSeconds(1)) {
						missingTimeRanges.Add(newTimeRange);
						previousEndTime = availableTimeRange.EndTime;
					}
				}

			}

			if (searchRange.EndTime > availableRanges.First().EndTime && previousEndTime >= availableRanges.First().EndTime) {
				missingTimeRanges.Add(new TimeRange(previousEndTime, searchRange.EndTime));
			}

			return missingTimeRanges.ToArray();

		}

		public static bool IsInTimeRange(DateTime dateToCheck, TimeRange range) {
			return dateToCheck >= range.StartTime && dateToCheck <= range.EndTime;
		}

		public static bool Contains(TimeRange rangeToCheck, TimeRange range) {
			bool startInRange = IsInTimeRange(rangeToCheck.StartTime, range);
			bool endInRange = IsInTimeRange(rangeToCheck.EndTime, range);
			return startInRange && endInRange;
		}

	}

}