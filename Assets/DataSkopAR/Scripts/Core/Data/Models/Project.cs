using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace DataskopAR.Data {

	[UsedImplicitly]
	public class Project {

#region Properties

		public int ID { get; set; }
		public ProjectInformation Information { get; set; }
		public AdditionalProperties Properties { get; set; }
		public ICollection<Device> Devices { get; set; }

#endregion

#region Constructors

		public Project(int id, ProjectInformation information, string additionalProperties) {

			ID = id;
			Information = information;
			Devices = new List<Device>();

			try {
				Properties = JsonConvert.DeserializeObject<AdditionalProperties>(additionalProperties);
			}
			catch {
				Properties = null;
			}

		}

#endregion

#region Methods

		public async Task UpdateDevices() {

			string url = $"https://backend.dataskop.at/api/project/measurementdefinitions/{ID}";
			string rawResponse = await ApiRequestHandler.Instance.Get(url);

			try {

				ICollection<MeasurementDefinition> projectMeasurementDefinitions =
					JsonConvert.DeserializeObject<ICollection<MeasurementDefinition>>(rawResponse);

				Devices = Build(projectMeasurementDefinitions);

			}
			catch {

				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Error,
					Text = $"Could not fetch MeasurementDefinitions of Project {ID}",
					DisplayDuration = NotificationDuration.Medium
				});

				return;

			}

			if (Devices?.Count == 0) {
				NotificationHandler.Add(new Notification {
					Category = NotificationCategory.Warning,
					Text = $"No Devices found in Project {ID}!",
					DisplayDuration = NotificationDuration.Medium
				});
			}

		}

		public async Task UpdateDeviceMeasurements(int fetchAmount) {

			foreach (Device d in Devices) {
				foreach (MeasurementDefinition md in d.MeasurementDefinitions) {
					await md.UpdateMeasurementResults(fetchAmount);
				}
			}

		}

		/// <returns>the latest TimeStamp from all measurements in the project.</returns>
		public DateTime GetLastUpdatedTime() {

			if (!(Devices?.Count > 0)) {
				return new DateTime();
			}

			List<DateTime> latestMeasurementTimes = new();

			foreach (Device d in Devices) {
				foreach (MeasurementDefinition md in d.MeasurementDefinitions) {

					if (md.GetLatestMeasurementResult() == null) {
						continue;
					}

					latestMeasurementTimes.Add(md.GetLatestMeasurementResult().Timestamp);
				}
			}

			return latestMeasurementTimes.OrderByDescending(x => x).FirstOrDefault();

		}

		private static ICollection<Device> Build(IEnumerable<MeasurementDefinition> projectMeasurementDefinitions) {

			ICollection<Device> devices = new List<Device>();

			foreach (MeasurementDefinition measurementDefinition in projectMeasurementDefinitions) {

				Device foundDevice = devices.FirstOrDefault(
					device => device.ID == measurementDefinition.DeviceId && measurementDefinition.DeviceId != null
				);

				if (foundDevice == null)
					devices.Add(new Device(
						measurementDefinition.DeviceId,
						measurementDefinition.DeviceId,
						new List<MeasurementDefinition> { measurementDefinition }
					));
				else {
					foundDevice.MeasurementDefinitions.Add(measurementDefinition);
				}

			}

			return devices;
		}

#endregion

	}

#region Sub-Classes

	[UsedImplicitly]
	public class ProjectInformation {

		public string Name { get; set; }

		public string Info { get; set; }

	}

	public class AdditionalProperties {

		public ICollection<DataAttribute> Attributes { get; set; }

		public bool IsDemo { get; set; }

	}

#endregion

}