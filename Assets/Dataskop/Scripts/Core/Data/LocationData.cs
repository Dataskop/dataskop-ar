using System;
using UnityEngine;

namespace Dataskop.Data {

	[CreateAssetMenu(fileName = "Location", menuName = "Locations/Add Location...", order = 1)]
	public class LocationData : ScriptableObject {
		
		[Serializable]
		public struct Area {

			public string areaName;
			[Tooltip("Format: lat, lon")] public string[] boundaryPoints;

		}

  

 

		public string locationName;
		public Area[] areas;

  

	}

}