using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dataskop.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dataskop.UI {

	public class InfoCardProjectSummary : MonoBehaviour {

		[SerializeField] private UIDocument infoCardDoc;
		
		private VisualElement ProjectSummaryContainer { get; set; }
		
		private Label ProjectName { get; set; }
		private Label ProjectDescription { get; set; }
		private Label ProjectTotalDevices { get; set; }
		private Label ProjectDevicesNearby { get; set; }
		private Label ProjectMeasurements { get; set; }

		private void Awake() {
			ProjectSummaryContainer = infoCardDoc.rootVisualElement.Q<VisualElement>("ProjectData");
			ProjectName = ProjectSummaryContainer.Q<Label>("NameValue");
			ProjectDescription = ProjectSummaryContainer.Q<Label>("DescriptionValue");
			ProjectTotalDevices = ProjectSummaryContainer.Q<Label>("TotalDevicesValue");
			ProjectDevicesNearby = ProjectSummaryContainer.Q<Label>("DevicesNearbyValue");
			ProjectMeasurements = ProjectSummaryContainer.Q<Label>("MeasurementsValue");
		}

		public void OnProjectLoaded(Project project) {
			ProjectName.text = project.Information.Name;
			ProjectDescription.text = project.Information.Description;
			ProjectTotalDevices.text = project.Devices.Count.ToString("00");
			
			string[] array = new string[project.Properties.Attributes.Count];
			for (int i = 0; i < array.Length; i++) {
				array[i] = project.Properties.Attributes.ToArray()[i].Label;
			}

			ProjectMeasurements.text = string.Join(", ", array);
		}

		public void OnNearbyDevicesUpdated(int devicesNearbyCount) {
			ProjectDevicesNearby.text = devicesNearbyCount.ToString("00");
		}

	}

}