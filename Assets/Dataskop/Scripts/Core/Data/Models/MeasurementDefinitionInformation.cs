namespace Dataskop.Data {

	public class MeasurementDefinitionInformation {

#region Constructor

		public MeasurementDefinitionInformation(string name, string uiShortName, string description, string info,
			string createdDate, string updatedDate) {
			Name = name;
			UiShortName = uiShortName;
			Description = description;
			Info = info;
			CreatedDate = createdDate;
			UpdatedDate = updatedDate;
		}

#endregion

#region Properties

		public string Name { get; set; }

		public string UiShortName { get; set; }

		public string Description { get; set; }

		public string Info { get; set; }

		public string CreatedDate { get; set; }

		public string UpdatedDate { get; set; }

#endregion

	}

}