namespace Dataskop.Data {

	public class MeasurementDefinitionInformation {

 

		public MeasurementDefinitionInformation(string name, string uiShortName, string description, string info,
			string createdDate, string updatedDate) {
			Name = name;
			UiShortName = uiShortName;
			Description = description;
			Info = info;
			CreatedDate = createdDate;
			UpdatedDate = updatedDate;
		}

  

 

		public string Name { get; set; }

		public string UiShortName { get; set; }

		public string Description { get; set; }

		public string Info { get; set; }

		public string CreatedDate { get; set; }

		public string UpdatedDate { get; set; }

  

	}

}