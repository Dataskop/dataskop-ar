using System;
using System.Collections.Generic;
using System.Linq;

namespace Dataskop.Data {

	public class Device {

		public string ID { get; set; }

		public string Label { get; set; }

		public ICollection<MeasurementDefinition> MeasurementDefinitions { get; set; }

		public DataAttribute[] Attributes { get; set; }

		/// <summary>
		/// Gets the position of a device on the earth.
		/// </summary>
		public Position Position => GetPosition();

		public Device(string id, string label, ICollection<MeasurementDefinition> measurementDefinitions) {

			ID = id;
			Label = label;
			MeasurementDefinitions = measurementDefinitions;

		}

		public MeasurementDefinition GetMeasurementDefinitionByID(int id) {
			return MeasurementDefinitions.First(item => item.ID == id);
		}

		public MeasurementDefinition GetMeasurementDefinitionByAttributeId(string attributeId) {
			return MeasurementDefinitions.First(item => item.AttributeId == attributeId);
		}

		public Dictionary<MeasurementDefinition, DateTime> GetLatestResultTimes() {
			return MeasurementDefinitions.ToDictionary(md => md, md => md.LatestMeasurementResult.Timestamp);
		}

		private Position GetPosition() {

			if (MeasurementDefinitions.Count == 0) {

				NotificationHandler.Add(
					new Notification {
						Category = NotificationCategory.Warning,
						Text = $"Device {ID} has no reported location!",
						DisplayDuration = NotificationDuration.Medium
					}
				);

				return null;
			}

			MeasurementResult result = MeasurementDefinitions?.First().LatestMeasurementResult;

			if (result != null) {
				return MeasurementDefinitions?.First().LatestMeasurementResult.Position;
			}

			NotificationHandler.Add(
				new Notification {
					Category = NotificationCategory.Warning,
					Text = $"Device {ID} has no reported location!",
					DisplayDuration = NotificationDuration.Medium
				}
			);

			return null;

		}

	}

}
