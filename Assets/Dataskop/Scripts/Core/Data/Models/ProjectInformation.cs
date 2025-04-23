using System;
using JetBrains.Annotations;

namespace Dataskop.Data {

	[UsedImplicitly]
	public class ProjectInformation {

		public string Name { get; set; }

		/// <summary>
		/// Combines Name and Description.
		/// </summary>
		public string Info { get; set; }

		public string Description { get; set; }

		public DateTime CreatedDate { get; set; }

		/// <summary>
		/// The Date the project was last updated.
		/// </summary>
		public DateTime UpdatedDate { get; set; }

	}

}