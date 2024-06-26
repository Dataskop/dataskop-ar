namespace Dataskop.Data {

	public class AdditionalMeasurementDefinitionProperties {

		public string DeviceId { get; }

		public string AttributeId { get; }

		public AdditionalMeasurementDefinitionProperties(string deviceId, string attributeId) {
			DeviceId = deviceId;
			AttributeId = attributeId;
		}

	}

}