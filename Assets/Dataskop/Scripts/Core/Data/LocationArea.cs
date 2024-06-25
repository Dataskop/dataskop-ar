using System.Collections.Generic;
using Mapbox.Utils;

namespace Dataskop.Data {

	public class LocationArea {

 

		public string LocationName { get; set; }

		public string AreaName { get; set; }

		public List<Vector2d> LatLonShapePoints { get; set; } = new();

  

	}

}