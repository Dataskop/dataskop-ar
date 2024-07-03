using System.Collections.Generic;
using Dataskop.Data;

namespace Dataskop {

	public class AdditionalProperties {

		public IReadOnlyCollection<DataAttribute> Attributes { get; }

		public bool IsDemo { get; }

		public AdditionalProperties(IReadOnlyCollection<DataAttribute> attributes, bool isDemo) {
			Attributes = attributes;
			IsDemo = isDemo;
		}

	}

}