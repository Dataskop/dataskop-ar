using System.Linq;
using Dataskop.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class InfoCardProjectSummary : InfoCardComponent {

		[SerializeField] private DataPointsManager dataPointsManager;

		protected override VisualElement ComponentRoot { get; set; }

		private VisualElement ProjectSummaryContainer { get; set; }

		private Label ProjectName { get; set; }

		private Label ProjectDescription { get; set; }

		private Label ProjectCreationDate { get; set; }

		private Label ProjectTotalDevices { get; set; }

		private Label ProjectDevicesNearbyLabel { get; set; }

		private Label ProjectDevicesNearbyValue { get; set; }

		private Label ProjectMeasurements { get; set; }

		public override void Init(VisualElement infoCard) {
			ComponentRoot = infoCard;
			ProjectSummaryContainer = ComponentRoot.Q<VisualElement>("ProjectSummary");
			ProjectName = ProjectSummaryContainer.Q<Label>("NameValue");
			ProjectDescription = ProjectSummaryContainer.Q<Label>("DescriptionValue");
			ProjectCreationDate = ProjectSummaryContainer.Q<Label>("CreationDateValue");
			ProjectTotalDevices = ProjectSummaryContainer.Q<Label>("TotalDevicesValue");
			ProjectDevicesNearbyLabel = ProjectSummaryContainer.Q<Label>("DevicesNearby");
			ProjectDevicesNearbyValue = ProjectSummaryContainer.Q<Label>("DevicesNearbyValue");
			ProjectMeasurements = ProjectSummaryContainer.Q<Label>("MeasurementsValue");
		}

		public void OnProjectLoaded(Project project) {

			ProjectName.text = project.Information.Name;
			ProjectDescription.text = project.Information.Description;
			ProjectCreationDate.text = project.Information.CreatedDate.ToShortDateString();
			ProjectTotalDevices.text = project.Devices.Count.ToString("00");
			ProjectDevicesNearbyLabel.text = $"Devices near you (within {dataPointsManager.NearbyDevicesDistance}m radius):";

			string[] array = new string[project.Properties.Attributes.Count];

			for (int i = 0; i < array.Length; i++) {
				array[i] = project.Properties.Attributes.ToArray()[i].Label;
			}

			ProjectMeasurements.text = string.Join(", ", array);

		}

		public void OnNearbyDevicesUpdated(int devicesNearbyCount) {
			ProjectDevicesNearbyValue.text = devicesNearbyCount.ToString("00");
		}

	}

}
