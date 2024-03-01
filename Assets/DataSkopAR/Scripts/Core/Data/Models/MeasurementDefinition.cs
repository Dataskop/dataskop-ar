using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace DataskopAR.Data {

	[UsedImplicitly]
	public class MeasurementDefinition {

#region Properties

		public int ID { get; }

		public MeasurementType MeasurementType { get; }

		public MeasurementDefinitionInformation MeasurementDefinitionInformation { get; }

		public string DeviceId { get; }

		public string AttributeId { get; }

		public ICollection<MeasurementResult> MeasurementResults { get; private set; }

#endregion

#region Constructors

		public MeasurementDefinition(int id, MeasurementDefinitionInformation information, string additionalProperties,
			int measurementInterval, int valueType, int downstreamType) {

			ID = id;
			MeasurementDefinitionInformation = information;

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

#endregion

#region Methods

		/// <summary>
		///     Fetches a list of measurement results belonging to the measurement definition.
		/// </summary>
		public async Task UpdateMeasurementResults(int count) {

			string countURL = $"https://backend.dataskop.at/api/measurementresult/query/{ID}/1/0";
			string countResponse = await ApiRequestHandler.Instance.Get(countURL);
			int totalCount;

			try {
				MeasurementResultsResponse response = JsonConvert.DeserializeObject<MeasurementResultsResponse>(countResponse);
				totalCount = response.Count;
			}
			catch {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch Measurement Results for Definition {ID}!",
					DisplayDuration = NotificationDuration.Medium,
				});
				throw;
			}

			if (count > totalCount) {

				count = totalCount;

				NotificationHandler.AddUnique(new Notification {
					Category = NotificationCategory.Warning,
					Text = $"Amount fetched too high. Clamping to {totalCount}!",
					DisplayDuration = NotificationDuration.Short,
					UniqueID = 2
				});

			}

			string url = $"https://backend.dataskop.at/api/measurementresult/query/{ID}/{count}/{totalCount - count}";
			string rawResponse = await ApiRequestHandler.Instance.Get(url);

			try {
				MeasurementResultsResponse response = JsonConvert.DeserializeObject<MeasurementResultsResponse>(rawResponse);
				MeasurementResults = response?.MeasurementResults.OrderByDescending(x => x.Timestamp).ToList();
			}
			catch {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch Measurement Results for Definition {ID}!",
					DisplayDuration = NotificationDuration.Medium
				});
				throw;
			}

		}

		/// <summary>
		///     Returns the most recent measurement result.
		/// </summary>
		public MeasurementResult GetLatestMeasurementResult() {
			return MeasurementResults?.FirstOrDefault();
		}

#endregion

	}

#region Sub-Classes

	public class AdditionalMeasurementDefinitionProperties {

		public string DeviceId { get; }

		public string AttributeId { get; }

		public AdditionalMeasurementDefinitionProperties(string deviceId, string attributeId) {
			DeviceId = deviceId;
			AttributeId = attributeId;
		}

	}

	public class MeasurementResultsResponse {

		public int Count { get; set; }

		public ICollection<MeasurementResult> MeasurementResults { get; }

		public MeasurementResultsResponse(int count, ICollection<MeasurementResult> measurementResults) {
			Count = count;
			MeasurementResults = measurementResults;
		}

	}

#endregion

}