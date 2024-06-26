using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Dataskop.Data {

	[UsedImplicitly]
	public class Project {

		public int ID { get; set; }

		public ProjectInformation Information { get; set; }

		public AdditionalProperties Properties { get; set; }

		public IReadOnlyCollection<Device> Devices { get; set; }

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

	}

}