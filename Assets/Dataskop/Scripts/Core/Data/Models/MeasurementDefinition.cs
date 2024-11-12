using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Dataskop.Data {

	[UsedImplicitly]
	public class MeasurementDefinition {

		public int ID { get; }

		public MeasurementType MeasurementType { get; }

		public MeasurementDefinitionInformation MeasurementDefinitionInformation { get; }

		public string DeviceId { get; }

		public string AttributeId { get; }

		public int MeasuringInterval { get; }

		public int TotalMeasurements { get; set; }

		public IReadOnlyList<MeasurementResultRange> MeasurementResults { get; private set; } =
			new List<MeasurementResultRange>();

		public MeasurementResult FirstMeasurementResult { get; set; }

		public MeasurementResult LatestMeasurementResult => GetLatestMeasurementResult();

		private float GapThreshold => MeasuringInterval / 2f;

		public MeasurementDefinition(int id, MeasurementDefinitionInformation information, string additionalProperties,
			int measurementInterval, int valueType, int downstreamType) {

			ID = id;
			MeasurementDefinitionInformation = information;
			MeasuringInterval = measurementInterval / 10;

			MeasurementType = valueType switch {
				0 => MeasurementType.Float,
				2 => MeasurementType.String,
				4 => MeasurementType.Bool,
				_ => throw new ArgumentOutOfRangeException(nameof(valueType), "Value type not supported.")
			};

			try {

				AdditionalMeasurementDefinitionProperties properties =
					JsonConvert.DeserializeObject<AdditionalMeasurementDefinitionProperties>(additionalProperties);

				DeviceId = properties.DeviceId;
				AttributeId = properties.AttributeId;

			}
			catch {

				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Warning,
						Text = $"Measurement Definition {ID} has no valid additional properties!",
						DisplayDuration = NotificationDuration.Medium
					}
				);

				DeviceId = null;
				AttributeId = null;

			}

		}

		private MeasurementResult GetLatestMeasurementResult() {
			return GetLatestRange().FirstOrDefault();
		}

		public MeasurementResultRange GetLatestRange() {
			return MeasurementResults.First();
		}

		/// <summary>
		/// Creates a new range from the available ranges with their data.
		/// </summary>
		/// <param name="timeRange">The given time range</param>
		/// <returns>A MeasurementResultRange with the given time range and results.</returns>
		public MeasurementResultRange GetRange(TimeRange timeRange) {

			MeasurementResultRange foundRange = new(Array.Empty<MeasurementResult>());

			foreach (MeasurementResultRange availableRange in MeasurementResults) {

				if (!TimeRangeExtensions.Contains(timeRange, availableRange.GetTimeRange())) {
					continue;
				}

				foundRange = availableRange;
				break;
			}

			MeasurementResultRange dataRange =
				new(
					foundRange.Where(x => x.Timestamp >= timeRange.StartTime && x.Timestamp <= timeRange.EndTime)
						.ToList()
				);

			dataRange.SetTimeRange(new TimeRange(timeRange.StartTime, timeRange.EndTime));
			return dataRange;
		}

		public bool IsDataGap(MeasurementResult result1, MeasurementResult result2) {
			TimeSpan timeDiff = result1.Timestamp - result2.Timestamp;
			TimeSpan interval = new(0, 0, MeasuringInterval);
			return Math.Truncate(Math.Abs(timeDiff.TotalSeconds)) > interval.TotalSeconds + GapThreshold;
		}

		public void AddMeasurementResultRange(MeasurementResultRange newRange, TimeRange timeRange) {

			List<MeasurementResultRange> currentRanges = MeasurementResults.ToList();
			newRange.SetTimeRange(timeRange);
			currentRanges.Add(newRange);
			MeasurementResults = currentRanges;
			SortRanges();

			if (MeasurementResults.Count < 2) {
				return;
			}

			MeasurementResults = GetMergedRanges();

		}

		private IReadOnlyList<MeasurementResultRange> GetMergedRanges() {

			List<MeasurementResultRange> mergedRanges = MeasurementResults.ToList();

			for (int i = 0; i < mergedRanges.Count - 1; i++) {
				MeasurementResultRange firstRange = mergedRanges[i];
				MeasurementResultRange secondRange = mergedRanges[i + 1];

				TimeRange firstTime = firstRange.GetTimeRange();
				TimeRange secondTime = secondRange.GetTimeRange();

				TimeSpan timeDifference = secondTime.EndTime - firstTime.StartTime;
				double totalSeconds = Math.Abs(timeDifference.TotalSeconds);
				double diff = Math.Abs(totalSeconds - MeasuringInterval);

				if (0 <= diff && diff <= MeasuringInterval) {

					bool hasDuplicate = false;

					if (firstRange.Any() && secondRange.Any()) {

						if (firstRange.Last() == secondRange.First()) {
							hasDuplicate = true;
						}

					}

					mergedRanges[i] =
						new MeasurementResultRange(
							hasDuplicate ? firstRange.SkipLast(1).Concat(secondRange)
								: firstRange.Concat(secondRange)
						);

					mergedRanges[i].SetTimeRange(
						new TimeRange(secondRange.GetTimeRange().StartTime, firstRange.GetTimeRange().EndTime)
					);

					mergedRanges.RemoveAt(i + 1);
					i--;

				}

			}

			return mergedRanges;
		}

		public TimeRange[] GetAvailableTimeRanges() {
			return MeasurementResults != null
				? MeasurementResults.Select(it => it.GetTimeRange()).ToArray()
				: Array.Empty<TimeRange>();
		}

		public IReadOnlyList<MeasurementResultRange> ReplaceMeasurementResultRange(int index,
			MeasurementResultRange newRange) {
			MeasurementResults.ToList()[index] = newRange;
			return MeasurementResults;
		}

		private void SortRanges() {
			MeasurementResults = MeasurementResults
				.OrderByDescending(mrr => mrr.GetTimeRange().StartTime)
				.ThenByDescending(mrr => mrr.GetTimeRange().EndTime)
				.ToList();
		}

	}

}