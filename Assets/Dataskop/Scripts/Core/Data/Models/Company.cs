using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Dataskop.Data {

	[UsedImplicitly]
	public class Company {

#region Constructors

		public Company(int id, CompanyInformation information, List<Project> companyProjects) {
			ID = id;
			Information = information;
			Projects = companyProjects;
		}

#endregion

#region Methods

		public async Task UpdateProjects() {

			string url = $"https://backend.dataskop.at/api/company/projects/{ID}";
			string rawResponse = await DataManager.RequestHandler.Get(url);

			try {
				Projects = JsonConvert.DeserializeObject<ICollection<Project>>(rawResponse);
			}
			catch (Exception e) {
				Debug.LogError(e.Message);
			}

		}

#endregion

#region Properties

		private int ID {
			get;
		}

		public CompanyInformation Information { get; set; }

		public ICollection<Project> Projects { get; private set; }

#endregion

	}

#region Sub-Classes

	[UsedImplicitly]
	public class CompanyInformation {

		public string Name { get; set; }

	}

#endregion

}