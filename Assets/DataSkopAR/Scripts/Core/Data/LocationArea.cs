using System.Collections.Generic;
using Mapbox.Utils;

namespace DataSkopAR.Data {

	public class LocationArea {

#region Properties

		public string LocationName { get; set; }
		public string AreaName { get; set; }
		public List<Vector2d> LatLonShapePoints { get; set; } = new();

#endregion

	}

}