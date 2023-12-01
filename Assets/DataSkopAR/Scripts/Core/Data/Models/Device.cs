using System.Collections.Generic;
using System.Linq;

namespace DataSkopAR.Data {

	public class Device {

#region Properties

		public string ID { get; set; }
		public string Label { get; set; }
		public ICollection<MeasurementDefinition> MeasurementDefinitions { get; set; }

		/// <summary>
		/// Gets the position of a device on the earth.
		/// </summary>
		public Position Position => GetPosition();

#endregion

#region Constructors

		public Device(string id, string label, ICollection<MeasurementDefinition> measurementDefinitions) {

			ID = id;
			Label = label;
			MeasurementDefinitions = measurementDefinitions;

		}

#endregion

#region Methods
		
		public MeasurementDefinition GetMeasurementDefinitionByID(int id) {
			return MeasurementDefinitions.First(item => item.ID == id);
		}
		
		public MeasurementDefinition GetMeasurementDefinitionByAttributeId(string attributeId) {
			return MeasurementDefinitions.First(item => item.AttributeId == attributeId);
		}

		private Position GetPosition() {

			if (MeasurementDefinitions.Count == 0) {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Warning,
					Text = $"Device {ID} has no reported location!",
					DisplayDuration = NotificationDuration.Medium
				});

				return null;
			}

			var result = MeasurementDefinitions?.First().GetLatestMeasurementResult();

			if (result == null) {
				
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Warning,
					Text = $"Device {ID} has no reported location!",
					DisplayDuration = NotificationDuration.Medium
				});
				
				return null;
			}

			return MeasurementDefinitions?.First().GetLatestMeasurementResult().Position;

		}

#endregion

	}

}