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

		public IReadOnlyCollection<MeasurementResult> MeasurementResults { get; set; }

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

		public MeasurementResult GetLatestMeasurementResult() {
			return MeasurementResults?.FirstOrDefault();
		}

	}

}