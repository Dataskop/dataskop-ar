using System.Collections.Generic;
using JetBrains.Annotations;

namespace Dataskop.Data {

	[UsedImplicitly]
	public class Company {

		public int ID { get; }

		public CompanyInformation Information { get; set; }

		public IReadOnlyCollection<Project> Projects { get; set; }

		public Company(int id, CompanyInformation information, List<Project> companyProjects) {
			ID = id;
			Information = information;
			Projects = companyProjects;
		}

	}

}