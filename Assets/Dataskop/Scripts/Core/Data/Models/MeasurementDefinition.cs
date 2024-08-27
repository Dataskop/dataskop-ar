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

		private float GapThreshold => MeasuringInterval / 2f;

		public int TotalMeasurements { get; set; }

		public IReadOnlyList<MeasurementResultRange> MeasurementResults { get; set; } = new List<MeasurementResultRange>();

		public MeasurementResult FirstMeasurementResult { get; set; }

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

		public MeasurementResult GetMeasurementResult(int index) {
			return GetAllResults().ToArray()[index];
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

		private IEnumerable<MeasurementResult> GetAllResults() {
			return MeasurementResults.SelectMany(x => x).ToArray();
		}

		public IEnumerable<MeasurementResult> GetMeasurementResults(DateTime from, DateTime to) {
			return GetAllResults().Where(mr => mr.Timestamp < from && mr.Timestamp > to).ToList();
		}

		public IReadOnlyList<MeasurementResultRange> AddMeasurementResultRange(MeasurementResultRange range) {

			//TODO: Append new range, merge with connecting range(s) and sort List of MeasurementResultRanges

			// Check if StartTime of new range is inside one of the ranges in the current List
			foreach (var mrr in MeasurementResults) {
				//if (range.StartTime => mrr.StartTime && range.EndTime <= )

			}

			SortRanges();
			return MeasurementResults;
		}

		/// <summary>
		/// Finds all gaps in the available Measurement Results given a from/to range.
		/// </summary>
		/// <param name="from">Startime of given time range</param>
		/// <param name="to">Endtime of given time range</param>
		/// <returns>Returns an array of time ranges.</returns>
		public TimeRange[] GetAllDataGaps() {

			List<TimeRange> newRanges = new();

			if (MeasurementResults.Count < 2) {
				// Only one range available - no gaps
				return newRanges.ToArray();
			}

			for (int i = 0; i < MeasurementResults.Count - 2; i++) {

				TimeRange tr = new(MeasurementResults[i].GetTimeRange().EndTime, MeasurementResults[i + 1].GetTimeRange().StartTime);
				newRanges.Add(tr);

			}

			return newRanges.ToArray();

		}

		public IReadOnlyList<MeasurementResultRange> ReplaceMeasurementResultRange(int index, MeasurementResultRange newRange) {
			MeasurementResults.ToList()[index] = newRange;
			return MeasurementResults;
		}

		public MeasurementResultRange GetLatestRange() {
			return MeasurementResults.First();
		}

		private void SortRanges() {
			MeasurementResults = MeasurementResults
				.OrderByDescending(mrr => mrr.GetTimeRange().StartTime)
				.ThenByDescending(mrr => mrr.GetTimeRange().EndTime)
				.ToList();
		}

	}

}