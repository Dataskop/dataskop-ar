using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

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

		public IReadOnlyList<MeasurementResultRange> MeasurementResults { get; private set; } = new List<MeasurementResultRange>();

		public MeasurementResult FirstMeasurementResult { get; set; }

		private float GapThreshold => MeasuringInterval / 2f;

		public MeasurementDefinition(int id, MeasurementDefinitionInformation information, string additionalProperties,
			int measurementInterval, int valueType, int downstreamType) {

			ID = id;
			MeasurementDefinitionInformation = information;
			MeasuringInterval = measurementInterval;

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

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Warning,
					Text = $"Measurement Definition {ID} has no valid additional properties!",
					DisplayDuration = NotificationDuration.Medium
				});

				DeviceId = null;
				AttributeId = null;

			}

		}

		public MeasurementResult GetLatestMeasurementResult() {
			return MeasurementResults.First()?.FirstOrDefault();
		}

		public MeasurementResultRange GetLatestRange() {
			return MeasurementResults.First();
		}

		public IEnumerable<MeasurementResult> GetMeasurementResults(TimeRange timeRange) {

			try {
				MeasurementResultRange foundRange = MeasurementResults
					.FirstOrDefault(x =>
						x.GetTimeRange().StartTime <= timeRange.StartTime && x.GetTimeRange().EndTime >= timeRange.EndTime);

				if (foundRange == null) {
					Debug.Log($"Could not find MeasurementResults for given TimeRange.");
				}

				return foundRange;
			}
			catch (InvalidOperationException e) when (MeasurementResults.Count == 0) {
				Debug.Log($"MeasurementResults collection is empty. {e.Message}");
				return null;
			}
			catch (Exception e) {
				Debug.Log($"Could not get MeasurementResults for given TimeRange: {e.Message}");
				return null;
			}

		}

		public MeasurementResult GetMeasurementResult(int index) {
			return GetLatestMeasurementResult();
		}

		public int? GetIndexOfMeasurementResult(MeasurementResult mr) {

			if (GetAllResults().Contains(mr)) {
				return Array.IndexOf(MeasurementResults.ToArray(), mr);
			}

			return null;

		}

		public bool IsDataGap(MeasurementResult result1, MeasurementResult result2) {
			TimeSpan timeDiff = result1.Timestamp - result2.Timestamp;
			TimeSpan interval = new(0, 0, MeasuringInterval / 10);
			return Math.Truncate(Math.Abs(timeDiff.TotalSeconds)) > interval.TotalSeconds + GapThreshold;
		}

		public void AddMeasurementResultRange(MeasurementResultRange newRange) {

			//TODO: Add new range, merge with connecting range(s) and sort List of MeasurementResultRanges

			// Check if StartTime of new range is inside one of the ranges in the current List
			if (newRange.Count < 1) {
				return;
			}

			List<MeasurementResultRange> currentRanges = MeasurementResults.ToList();
			currentRanges.Add(newRange);
			MeasurementResults = currentRanges;
			SortRanges();

			if (MeasurementResults.Count < 2) {
				return;
			}

			MeasurementResults = TryMergingExistingRanges();

		}

		private IReadOnlyList<MeasurementResultRange> TryMergingExistingRanges() {
			List<MeasurementResultRange> mergedRanges = MeasurementResults.ToList();

			for (int i = 0; i < mergedRanges.Count - 1; i++) {
				MeasurementResultRange firstRange = mergedRanges[i];
				MeasurementResultRange secondRange = mergedRanges[i + 1];

				TimeSpan timeDifference = secondRange.GetTimeRange().EndTime - firstRange.GetTimeRange().StartTime;

				if (timeDifference.TotalSeconds <= MeasuringInterval / 10f && firstRange.Last() == secondRange.First()) {
					mergedRanges[i] = new(firstRange.Concat(secondRange.Skip(1)));
					mergedRanges.RemoveAt(i + 1);
					i--;
				}
			}

			return mergedRanges;
		}

		private TimeRange[] GetAvailableTimeRanges() {

			TimeRange[] availableRanges = Array.Empty<TimeRange>();

			if (MeasurementResults != null) {
				availableRanges = new TimeRange[MeasurementResults.Count];

				for (int i = 0; i < availableRanges.Length; i++) {
					var resultTime = MeasurementResults[i].GetTimeRange();
					availableRanges[i] = new TimeRange(resultTime.StartTime, resultTime.EndTime);
				}

			}

			return availableRanges;
		}

		public TimeRange[] GetMissingTimeRanges(TimeRange newTimeRange) {

			TimeRange[] availableTimeRanges = GetAvailableTimeRanges();
			List<TimeRange> missingTimeRanges = new();
			DateTime previousEndTime = newTimeRange.StartTime;

			for (int i = availableTimeRanges.Length - 1; i >= 0; i--) {
				TimeRange availableTimeRange = availableTimeRanges[i];

				if (newTimeRange.StartTime > availableTimeRange.EndTime) {
					continue;
				}

				if (newTimeRange.EndTime >= availableTimeRange.StartTime && newTimeRange.EndTime <= availableTimeRange.EndTime) {

					if (newTimeRange.EndTime > previousEndTime) {
						missingTimeRanges.Add(new TimeRange(previousEndTime, availableTimeRange.StartTime));
					}

					break;
				}

				if (newTimeRange.EndTime <= availableTimeRange.StartTime) {
					missingTimeRanges.Add(new TimeRange(previousEndTime, newTimeRange.EndTime));
				}
				else {
					missingTimeRanges.Add(new TimeRange(previousEndTime, availableTimeRange.StartTime));
					previousEndTime = availableTimeRange.EndTime;
				}

			}

			return missingTimeRanges.ToArray();
		}

		public IReadOnlyList<MeasurementResultRange> ReplaceMeasurementResultRange(int index, MeasurementResultRange newRange) {
			MeasurementResults.ToList()[index] = newRange;
			return MeasurementResults;
		}

		private void SortRanges() {
			MeasurementResults = MeasurementResults
				.OrderByDescending(mrr => mrr.GetTimeRange().StartTime)
				.ThenByDescending(mrr => mrr.GetTimeRange().EndTime)
				.ToList();
		}

		private IEnumerable<MeasurementResult> GetAllResults() {
			return MeasurementResults.SelectMany(x => x).ToArray();
		}

	}

}